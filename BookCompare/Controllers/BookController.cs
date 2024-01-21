using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using BookCompare.CookiesModel;
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
            var userId = User.Identity.GetUserId(); //null if user is off
            var sortedBookViewModels = await booksService.GetAllBooks(bookName,userId,Request);
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
            if (!ModelState.IsValid) return Json(new { success = false });

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                WishlistItem wishlistItem = booksService.AddToWishlist(userId, bookViewModel);
                if (wishlistItem == null)
                {
                    return Json(new { success = false });
                }
            }
            else // User is not logged in, add the item to the cookie-based wishlist
            {
                WishlistItemCookie wishlistCookieItem = booksService.AddToCookieWishlist(Request, Response, bookViewModel);
                if (wishlistCookieItem == null)
                {
                    return Json(new { success = false });
                }
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult RemoveFromWishlist(BookViewModel bookViewModel)
        {
            if (!ModelState.IsValid) return Json(new { success = false });

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                WishlistItem wishlistItem = booksService.RemoveFromWishlist(userId, bookViewModel);
                if (wishlistItem == null)
                {
                    return Json(new { success = false });
                }
            }
            else
            {
                // User is not logged in, remove the item to the cookie-based wishlist
                WishlistItemCookie wishlistCookieItem = booksService.RemoveFromCookieWishlist(Request, Response, bookViewModel);
                if (wishlistCookieItem == null)
                {
                    return Json(new { success = false });
                }
            }
           
            return Json(new { success = true });

        }


    }
}