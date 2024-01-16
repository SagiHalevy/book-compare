    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    namespace BookCompare.ViewModels
    {
        public class BookViewModel
        {
            public string Origin {  get; set; }
            public string Title { get; set; }
            public string ImageUrl { get; set; }
            public double? Price { get; set; }
            public string BuyUrl { get; set; }
            public string OriginImageUrl { get; set;}
        }
    }