using BookCompare.DataAccess;
using BookCompare.ScrapersNew;
using BookCompare.ViewModels;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BookCompare.Services
{
    public class BooksService
    {
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



        public async Task<List<BookViewModel>> GetAllBooks(string bookName)
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
            var sortedBookViewModels = bookViewModels.OrderBy(book => book.Price ?? double.MaxValue).ToList();

            return sortedBookViewModels;
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


    }
}