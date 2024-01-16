using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BookCompare.ScrapersNew
{
    public class Kidsbest : Scraper
    {
        protected override string OriginName => "קידסבסט";

        protected override string OriginImageUrl => "~/Content/Images/Kidsbest.jpg";

        protected override string OriginSearchUrl => "https://kidsbest.co.il/?post_type=product&dgwt_wcas=1&s=";

        protected override string ContainerNodeXPath => "//div[@class='entry-content']//ul//li//div[@class='basic-data']";

        protected override string TitleXPath => "//div[@class='bestkids-product-grid-title']//a//span[@class='woocommerce-loop-product__title']";

        protected override string PriceXPath => "//div[@class='cat-price']//span[@class='price']//ins//bdi";

        protected override string ImageUrlXPath => ".//a//img[contains(@class,'attachment-woocommerce_thumbnail')]";

        protected override string ImageUrlAttribute => "src";

        protected override string OriginUrlXPath => "//div[@class='bestkids-product-grid-title']//a";

        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://kidsbest.co.il/";


        //override because the scraped title had special characters that had to be decoded
        protected override string GetTitle(HtmlNode containerNode)
        {
            var bookTitleNode = containerNode.SelectSingleNode(TitleXPath);
            string decodedTitle = bookTitleNode?.InnerText ?? "Unknown Title";
            decodedTitle = HttpUtility.HtmlDecode(decodedTitle);
            return decodedTitle;
        }

        //override because the scraped price had special characters that had to be decoded
        protected override double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectSingleNode(PriceXPath);
            if (priceNode == null) return null;

            var trimmedPrice = priceNode.InnerText.Trim();
            string decodedPrice = HttpUtility.HtmlDecode(trimmedPrice);
            string price = Regex.Replace(decodedPrice, @"[^\d.]", "");

            // Convert the cleaned text to a numeric type
            return double.TryParse(price, out double numericPrice) ? numericPrice : (double?)null;
        }
    }
}