using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BookCompare.DataAccess;
using BookCompare.Models;
using BookCompare.ScrapersNew;
using BookCompare.Services;
using BookCompare.ViewModels;
using HtmlAgilityPack;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace BookCompare.Controllers
{
    public class BookController : Controller
    {
        private ApplicationDbContext _context;

        public BookController()
        {
            _context = new ApplicationDbContext();
        }

        static BooksService booksService = new BooksService(); 

        // GET: Book
        public ActionResult Index()
        {
            return View();
        }

        // Get: Book
        public async Task<ActionResult> Search(string bookName)
        {
            var sortedBookViewModels = await booksService.GetAllBooks(bookName);
            ViewBag.SearchInput = bookName;
            return View("Book", sortedBookViewModels);
        }

        // GET: Book
        public ActionResult Book()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddToWishlist(BookViewModel bookViewModel)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "User is not authenticated." });
            }

            if (!ModelState.IsValid) return Json(new { success = false });
            
            string uniqueString = $"{bookViewModel.Origin}{bookViewModel.Title}{bookViewModel.BuyUrl}";
            var hashedId = ComputeFNVHash(uniqueString);
            var userId = User.Identity.GetUserId();

            var alreadyExistInWishlist = _context.WishlistItems.Where(i => i.UserId == userId && i.HashedId == hashedId).FirstOrDefault();
            if(alreadyExistInWishlist != null)
            {
                return Json(new { success = false });
            }

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
            return Json(new { success = true });
           
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