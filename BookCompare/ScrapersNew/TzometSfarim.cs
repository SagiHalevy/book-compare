using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookCompare.ScrapersNew
{
    public class TzometSfarim :Scraper
    {
        protected override string OriginName => "צומת ספרים";
        protected override string OriginImageUrl => "~/Content/Images/TzometSfarim.jpg";
        protected override string OriginSearchUrl => "https://www.booknet.co.il/%D7%97%D7%99%D7%A4%D7%95%D7%A9/?q=";

        protected override string ContainerNodeXPath => "//div[@class='categoryitems']//div[contains(@class,'product-cube')]";

        protected override string TitleXPath => "//div[@class='book-item']//a//h3[@class='productTitle']";
        protected override string PriceXPath => "//div[@class='price']//ins";
        protected override string ImageUrlXPath => "//div[@class='productImage']//img";
        protected override string ImageUrlAttribute => "data-original";
        protected override string OriginUrlXPath => "//div[@class='book-item']//a";
        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://www.booknet.co.il/";



    }
}