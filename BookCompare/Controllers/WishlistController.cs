using BookCompare.Models;
using Microsoft.AspNet.Identity;
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
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var wishlist = _context.WishlistItems.Where(u => u.UserId == userId).ToList();


            return View(wishlist);
        }
    }
}