using BookCompare.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BookCompare.Scrapers
{
    public class DaniSfarim : IScraper
    {
        static HtmlWeb web = new HtmlWeb();
        public BookViewModel GetBookData(string bookName)
        {
            var encodedBookName = HttpUtility.UrlEncode(bookName);
            var url = $"https://www.siman-kria.co.il/Search/?q={encodedBookName}";
            var doc = web.Load(url);
            var containerNode = doc.DocumentNode.SelectSingleNode("//div[@id='SearchResultPage']//ul[@class='searchResultsList']");

            if (containerNode == null) return null;

            return new BookViewModel()
            {
                Origin = "דני ספרים",
                Title = GetBookTitle(containerNode) ?? bookName,
                ImageUrl = GetImageUrl(containerNode),
                Price = GetPrice(containerNode),
                BuyUrl = GetBookUrl(containerNode),
            };
        }


        private string GetImageUrl(HtmlNode containerNode)
        {
            var bookImg = containerNode.SelectSingleNode("//div[contains(@class,'catalogItemBox')]//a[@class='item-image ']//img");
            return bookImg?.GetAttributeValue("src", "") ?? string.Empty;

        }
    
        private double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectSingleNode("//div[contains(@class,'catalogItemBox')]//a[@class='item-text']//span[@class='finalPrice']");
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
            var bookUrlNode = containerNode.SelectSingleNode("//div[contains(@class,'catalogItemBox')]//a[@class='item-text']");
            string hrefValue = bookUrlNode?.GetAttributeValue("href", "");

            if (!string.IsNullOrEmpty(hrefValue))
            {
                return "https://www.siman-kria.co.il/" + hrefValue;
            }

            return string.Empty;
        }

        private string GetBookTitle(HtmlNode containerNode)
        {
            var bookTitleNode = containerNode.SelectSingleNode("//div[contains(@class,'catalogItemBox')]//a[@class='item-text']//h3[class='itemTitle']");
            return bookTitleNode?.InnerText.Trim() ?? null;
        }

    }
}