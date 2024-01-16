using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BookCompare.ScrapersNew
{
    public class Steimatzky : Scraper
    {
        protected override string OriginName => "סטימצקי";
        protected override string OriginImageUrl => "~/Content/Images/Steimatzki.jpg";
        protected override string OriginSearchUrl => "https://www.steimatzky.co.il/catalogsearch/result/?q=";

        protected override string ContainerNodeXPath => "//li[contains(@class,'product-item')]";

        protected override string TitleXPath => ".//a[contains(@class, 'product_link')]";

        protected override string PriceXPath => ".//span[contains(@data-price-type,'finalPrice')]";

        protected override string ImageUrlXPath => "//div[@class='image']//img[@class='product-image-photo']";

        protected override string ImageUrlAttribute => "src";

        protected override string OriginUrlXPath => ".//a[contains(@class, 'product_link')]";

        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://www.steimatzky.co.il/";



        //overrided because in this case there is <a> that contains 'title' attribute
        protected override string GetTitle(HtmlNode containerNode)
        {
            var bookTitleNode = containerNode.SelectSingleNode(TitleXPath);
            return bookTitleNode?.GetAttributeValue("title", "") ?? "Unknown Title";
        }


        //overrided because scraping is different when there is 'Digital Product' as well
        protected override double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectNodes(PriceXPath);
            if (priceNode == null) return null;

            //if priceNode>2 then priceNode[0] is the 'Digital Product' price and priceNode[1] is the 'Regular Book'
            string trimmedPrice = priceNode.Count >= 2 ? priceNode[1].InnerText.Trim() : priceNode[0].InnerText.Trim();

            //regex to get only the number value
            string price = Regex.Replace(trimmedPrice, @"[^\d.]", "");
            return double.TryParse(price, out double numericPrice) ? numericPrice : (double?)null;

        }



    }
}