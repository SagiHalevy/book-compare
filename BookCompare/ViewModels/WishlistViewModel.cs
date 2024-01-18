using BookCompare.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookCompare.ViewModels
{
    public interface IWishlistViewModel
    {
        DateTime DateAdded { get; set; }
        string Origin { get; set; }
        string Title { get; set; }
        string ImageUrl { get; set; }
        double? Price { get; set; }
        string BuyUrl { get; set; }
        string OriginImageUrl { get; set; }
    }
}