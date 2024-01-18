using BookCompare.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BookCompare.ViewModels;

namespace BookCompare.CookiesModel
{
    public class WishlistItemCookie : IWishlistViewModel
    {
        public string HashedId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Origin { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public double? Price { get; set; }
        public string BuyUrl { get; set; }
        public string OriginImageUrl { get; set; }
    }
}