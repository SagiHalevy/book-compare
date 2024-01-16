using BookCompare.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BookCompare.Scrapers
{
    public class TzometSfarim : IScraper
    {
        static HtmlWeb web = new HtmlWeb();

        public BookViewModel GetBookData(string bookName)
        {
            
            var encodedBookName = HttpUtility.UrlEncode(bookName);
            var url = $"https://www.booknet.co.il/%D7%97%D7%99%D7%A4%D7%95%D7%A9/?q={encodedBookName}";
            var doc = web.Load(url);
            var containerNode = doc.DocumentNode.SelectSingleNode("//div[@class='categoryitems']");
        
            if (containerNode == null) return null;

            return new BookViewModel()
            {
                Origin = "צומת ספרים",
                Title = GetBookTitle(containerNode) ?? bookName,
                ImageUrl = GetImageUrl(containerNode),
                Price = GetPrice(containerNode),
                BuyUrl = GetBookUrl(containerNode),
            };
        }


        private string GetImageUrl(HtmlNode containerNode)
        {
            var bookImg = containerNode.SelectSingleNode("//div[@class='productImage']//img");
            
            string srcValue = bookImg?.GetAttributeValue("data-original", "");

            return !string.IsNullOrEmpty(srcValue) ? "https://www.booknet.co.il/" + srcValue : string.Empty;
        }

        private double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectSingleNode("//div[@class='price']//ins");
            if (priceNode == null) return null;

            var trimmedPrice = priceNode.InnerText.Trim();
            string price = Regex.Replace(trimmedPrice, @"[^\d.]", "");

            // Convert the cleaned text to a numeric type
            if (double.TryParse(price, out double numericPrice))
            {
                return numericPrice;
            }
            else
            {
                return null;
            }
        }



        private string GetBookUrl(HtmlNode containerNode)
        {
            var bookUrlNode = containerNode.SelectSingleNode("//div[@class='book-item']//a");

            string hrefValue = bookUrlNode?.GetAttributeValue("href", "");

            return !string.IsNullOrEmpty(hrefValue) ? "https://www.booknet.co.il/" + hrefValue : string.Empty;

        }

        private string GetBookTitle(HtmlNode containerNode)
        {
            var bookTitleNode = containerNode.SelectSingleNode("//div[@class='book-item']//a//h3[@class='productTitle']");
            return bookTitleNode?.InnerText ?? null;
        }

    }
}