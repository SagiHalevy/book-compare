using BookCompare.ViewModels;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BookCompare.Scrapers
{

    public class Steimatzky : IScraper
    {
        static HtmlWeb web = new HtmlWeb();

        public BookViewModel GetBookData(string bookName)
        {
            var encodedBookName = HttpUtility.UrlEncode(bookName);
            var url = $"https://www.steimatzky.co.il/catalogsearch/result/?q={encodedBookName}";
            var doc = web.Load(url);
            var containerNode = doc.DocumentNode.SelectSingleNode("//li[contains(@class,'product-item')]");

            if (containerNode == null) return null;

            return new BookViewModel()
            {
                Origin = "סטימצקי",
                Title = GetBookTitle(containerNode) ?? bookName,
                ImageUrl = GetImageUrl(containerNode),
                Price = GetPrice(containerNode),
                BuyUrl = GetBookUrl(containerNode),
            };
        }
        private string GetImageUrl(HtmlNode containerNode)
        {
            var bookImg = containerNode.SelectSingleNode("//div[@class='image']//img[@class='product-image-photo']");
            return bookImg?.GetAttributeValue("src", "") ?? string.Empty;
        }

        private double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectNodes(".//span[contains(@data-price-type,'finalPrice')]");
            string trimmedPrice;

            if (priceNode == null) return null;

            if (priceNode.Count >= 2)
            {
                //var digitalPrice = priceNode[0].InnerText.Trim();
                trimmedPrice = priceNode[1].InnerText.Trim();

                //return $"Digital Price: {digitalPrice}\nRegular Price: {regularPrice}";
            }
            else
            {
                trimmedPrice = priceNode[0].InnerText.Trim();
            }
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
            var bookUrlNode = containerNode.SelectSingleNode(".//a[contains(@class, 'product_link')]");
            return bookUrlNode?.GetAttributeValue("href", "") ?? null;
        }

        private string GetBookTitle(HtmlNode containerNode)
        {
            var bookTitleNode = containerNode.SelectSingleNode(".//a[contains(@class, 'product_link')]");
            return bookTitleNode?.GetAttributeValue("title", "") ?? null;
        }

    }
}