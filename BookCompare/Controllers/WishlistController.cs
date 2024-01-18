using BookCompare.CookiesModel;
using BookCompare.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookCompare.Controllers
{
    public class WishlistController : Controller
    {
        private ApplicationDbContext _context;

        public WishlistController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Wishlist
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated) 
            { 
                var userId = User.Identity.GetUserId();
                var wishlist = _context.WishlistItems.Where(u => u.UserId == userId).ToList();
                return View(wishlist);
            }
            else //Cookies
            {
                var wishlistCookie = Request.Cookies["Wishlist"];
                if (wishlistCookie == null || string.IsNullOrEmpty(wishlistCookie.Value)) 
                {
                    return View(new List<WishlistItemCookie>());
                }

                var wishlistJson = HttpUtility.UrlDecode(wishlistCookie.Value);
                var wishlistItemCookieList = JsonConvert.DeserializeObject<List<WishlistItemCookie>>(wishlistJson);

                return View(wishlistItemCookieList);
            }
     
        }
    }
}