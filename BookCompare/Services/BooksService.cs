using BookCompare.CookiesModel;
using BookCompare.DataAccess;
using BookCompare.Models;
using BookCompare.ScrapersNew;
using BookCompare.ViewModels;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BookCompare.Services
{
    public class BooksService
    {
        private ApplicationDbContext _context;

        public BooksService()
        {
            _context = new ApplicationDbContext();
        }

        static List<Scraper> origins = new List<Scraper>
        {
            new Steimatzky(),
            new TzometSfarim(),
            new DaniSfarim(),
            new Sipurpashut(),
            new Kidsbest(),
            new Obook(),
        };

        private const int TimeoutInSeconds = 100; //timeout for maximum search time



        public async Task<List<BookViewModel>> GetAllBooks(string bookName, string userId, HttpRequestBase request)
        {

            //running all scraping simultaneously
            var scrapeTasks = origins.Select(async origin =>
            {
                var cts = new CancellationTokenSource();
                var scrapeTask = origin.GetScrapedDataAsync(bookName, cts.Token);

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds));
                var completedTask = await Task.WhenAny(scrapeTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    cts.Cancel(); // Cancel the scraping task if it times out
                    return null;  // Origin will be skipped
                }
                return await scrapeTask; // return the data from the scrapeTask
            });

            var bookDataResults = await Task.WhenAll(scrapeTasks);

            var bookViewModels = bookDataResults.Where(bookData => bookData != null).ToList();

            bookViewModels = bookViewModels.OrderBy(book => book.Price ?? double.MaxValue).ToList();

            if(userId != null) //user is authenticated
            {
                foreach (var bookViewModel in bookViewModels)
                {
                    if (GetBookIfExistInWishlist(bookViewModel, userId) != null)
                    {
                        bookViewModel.IsInWishlist = true;
                    }
                }
            }
            else //user is not authenticated
            {
                var wishlistCookie = request.Cookies["Wishlist"];
                if (wishlistCookie != null && !string.IsNullOrEmpty(wishlistCookie.Value)) 
                {
                    var wishlistJson = HttpUtility.UrlDecode(wishlistCookie.Value);
                    var wishlistItemCookieList = JsonConvert.DeserializeObject<List<WishlistItemCookie>>(wishlistJson);
                    foreach (var bookViewModel in bookViewModels)
                    {
                        if (GetBookIfExistInCookieWishlist(bookViewModel, wishlistItemCookieList) != null)
                        {
                            bookViewModel.IsInWishlist = true;
                        }
                    }
                }
            }
            return bookViewModels;
        }



        /// <summary>
        /// Scraping books data from all the origins or gets them from Redis if exist.
        /// </summary>
        /// <param name="bookName"></param>
        /// <returns>Sorted list of all the books found</returns>
        public async Task<List<BookViewModel>> GetAllBooksUseRedis(string bookName)
        {

            string redisKey = "Book:" + bookName;
            var redisDatabase = RedisConnectionFactory.Connection.GetDatabase();
            var cachedData = await redisDatabase.StringGetAsync(redisKey);

            if (!cachedData.IsNull)
            {
                // Data found in Redis, deserialize and return it
                var cachedBookViewModels = JsonConvert.DeserializeObject<List<BookViewModel>>(cachedData);
                return cachedBookViewModels;
            }

            //running all scraping simultaneously
            var scrapeTasks = origins.Select(async origin =>
            {
                var cts = new CancellationTokenSource();
                var scrapeTask = origin.GetScrapedDataAsync(bookName, cts.Token);

                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(TimeoutInSeconds));
                var completedTask = await Task.WhenAny(scrapeTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    cts.Cancel(); // Cancel the scraping task if it times out
                    return null;  // Origin will be skipped
                }
                return await scrapeTask; // return the data from the scrapeTast
            });

            var bookDataResults = await Task.WhenAll(scrapeTasks);

            var bookViewModels = bookDataResults.Where(bookData => bookData != null).ToList();
            var sortedBookViewModels = bookViewModels.OrderBy(book => book.Price ?? double.MaxValue).ToList();

            //save to Redis
            var serializedData = JsonConvert.SerializeObject(sortedBookViewModels);
            await redisDatabase.StringSetAsync(redisKey, serializedData, TimeSpan.FromHours(1));

            return sortedBookViewModels;
        }





        public WishlistItem AddToWishlist(string userId, BookViewModel bookViewModel)
        {
            var bookInDb = GetBookIfExistInWishlist(bookViewModel, userId);
            if (bookInDb != null)
            {
                return null;
            }

            string uniqueString = $"{bookViewModel.Origin}{bookViewModel.Title}{bookViewModel.BuyUrl}";
            var hashedId = ComputeFNVHash(uniqueString);
            WishlistItem wishlistItem = new WishlistItem
            {
                HashedId = hashedId,
                UserId = userId,
                DateAdded = DateTime.Now,
                Origin = bookViewModel.Origin,
                Title = bookViewModel.Title,
                ImageUrl = bookViewModel.ImageUrl,
                Price = bookViewModel.Price,
                BuyUrl = bookViewModel.BuyUrl,
                OriginImageUrl = bookViewModel.OriginImageUrl,
            };
            _context.WishlistItems.Add(wishlistItem);
            _context.SaveChanges();
            return wishlistItem;
        }


        public WishlistItem RemoveFromWishlist(string userId, BookViewModel bookViewModel)
        {
            var bookInDb = GetBookIfExistInWishlist(bookViewModel, userId);
            if (bookInDb == null)
            {
                return null;
            }
            _context.WishlistItems.Remove(bookInDb);
            _context.SaveChanges();
            return bookInDb;
        }


        private WishlistItem GetBookIfExistInWishlist(BookViewModel bookViewModel, string userId)
        {
            string uniqueString = $"{bookViewModel.Origin}{bookViewModel.Title}{bookViewModel.BuyUrl}";
            var hashedId = ComputeFNVHash(uniqueString);
            var wishlistItem = _context.WishlistItems.Where(i => i.UserId == userId && i.HashedId == hashedId).FirstOrDefault();

            if (wishlistItem == null) return null;
            return wishlistItem;
           
        }

    






        public WishlistItemCookie AddToCookieWishlist(HttpRequestBase request, HttpResponseBase response, BookViewModel bookViewModel)
        {
            var wishlistCookie = request.Cookies["Wishlist"];
            List<WishlistItemCookie> wishlistItemCookieList;
            if (wishlistCookie == null)
            {
                wishlistItemCookieList = new List<WishlistItemCookie>();
            }
            else
            {
                var wishlistJson = HttpUtility.UrlDecode(wishlistCookie.Value);
                wishlistItemCookieList = JsonConvert.DeserializeObject<List<WishlistItemCookie>>(wishlistJson);
            }

            var bookInCookie = GetBookIfExistInCookieWishlist(bookViewModel, wishlistItemCookieList);
            if (bookInCookie != null) return null;

            string uniqueString = $"{bookViewModel.Origin}{bookViewModel.Title}{bookViewModel.BuyUrl}";
            var hashedId = ComputeFNVHash(uniqueString);

            WishlistItemCookie wishlistCookieItem = new WishlistItemCookie
            {
                HashedId = hashedId,
                DateAdded = DateTime.Now,
                Origin = bookViewModel.Origin,
                Title = bookViewModel.Title,
                ImageUrl = bookViewModel.ImageUrl,
                Price = bookViewModel.Price,
                BuyUrl = bookViewModel.BuyUrl,
                OriginImageUrl = bookViewModel.OriginImageUrl,
            };
            wishlistItemCookieList.Add(wishlistCookieItem);
            SaveWishlistToCookie(wishlistItemCookieList, response);
            return wishlistCookieItem;
            
        }

        public WishlistItemCookie RemoveFromCookieWishlist(HttpRequestBase request, HttpResponseBase response, BookViewModel bookViewModel)
        {
            var wishlistCookie = request.Cookies["Wishlist"];
            List<WishlistItemCookie> wishlistItemCookieList;
            if (wishlistCookie == null)
            {
                wishlistItemCookieList = new List<WishlistItemCookie>();
            }
            else
            {
                var wishlistJson = HttpUtility.UrlDecode(wishlistCookie.Value);
                wishlistItemCookieList = JsonConvert.DeserializeObject<List<WishlistItemCookie>>(wishlistJson);
            }
            var bookInCookie = GetBookIfExistInCookieWishlist(bookViewModel, wishlistItemCookieList);
            if (bookInCookie == null) return null;
          
            wishlistItemCookieList.Remove(bookInCookie);
            SaveWishlistToCookie(wishlistItemCookieList, response);
            return bookInCookie;
        }


        public void SaveWishlistToCookie(List<WishlistItemCookie> wishlist, HttpResponseBase response)
        {
            var wishlistJson = JsonConvert.SerializeObject(wishlist);
            var cookie = new HttpCookie("Wishlist")
            {
                Value = HttpUtility.UrlEncode(wishlistJson),
                Expires = DateTime.Now.AddDays(7) // Adjust the expiration as needed
            };
            response.Cookies.Add(cookie);
        }


        private WishlistItemCookie GetBookIfExistInCookieWishlist(BookViewModel bookViewModel, List<WishlistItemCookie> bookViewModelList)
        {
            string uniqueString = $"{bookViewModel.Origin}{bookViewModel.Title}{bookViewModel.BuyUrl}";
            var hashedId = ComputeFNVHash(uniqueString);
            var wishlistItem = bookViewModelList.Where(i => i.HashedId == hashedId).FirstOrDefault();

            if (wishlistItem == null) return null;
            return wishlistItem;

        }





        private string ComputeFNVHash(string input)
        {
            const uint fnvPrime = 16777619; // FNV prime number
            const uint fnvOffsetBasis = 2166136261; // FNV offset basis

            uint hash = fnvOffsetBasis;

            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                hash ^= b;
                hash *= fnvPrime;
            }

            return hash.ToString("X8"); // Convert to hexadecimal representation
        }





    }
}