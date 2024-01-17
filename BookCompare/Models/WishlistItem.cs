using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookCompare.Models
{
    public class WishlistItem
    {
        [Key, Column(Order = 0)]
        public string HashedId { get; set; } // Composite primary key with UserId 

        [Key, Column(Order = 1)]
        public string UserId {  get; set; } // Foreign key to ApplicationUser

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // ApplicationUser is the Identity User class

        public DateTime DateAdded { get; set; }
        public string Origin { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public double? Price { get; set; }
        public string BuyUrl { get; set; }
        public string OriginImageUrl { get; set; }
    }
}