using BookCompare.DataAccess;
using BookCompare.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace BookCompare.ScrapersNew
{
    public abstract class Scraper
    {
        static HtmlWeb web = new HtmlWeb();
        protected abstract string OriginName { get;}
        protected abstract string OriginImageUrl{ get;}
        protected abstract string OriginSearchUrl { get;}

        protected abstract string ContainerNodeXPath { get;}

        protected abstract string TitleXPath { get;}
        protected abstract string PriceXPath { get;}
        protected abstract string ImageUrlXPath { get;}
        protected abstract string ImageUrlAttribute { get; }
        protected abstract string OriginUrlXPath { get;}
        protected abstract string OriginUrlAttribute { get;}

        protected abstract string OriginBaseUrl { get;}

        protected const string NotFoundImage = "~/Content/Images/NotFound.jpg";

        public virtual async Task<BookViewModel> GetScrapedDataAsync(string bookName, CancellationToken cancellationToken)
        {
            var encodedBookName = Uri.EscapeDataString(bookName);
            var url = $"{OriginSearchUrl}{encodedBookName}";

            // Check if the cancellation token has been triggered before making the web request
            cancellationToken.ThrowIfCancellationRequested();
            var doc = await web.LoadFromWebAsync(url, cancellationToken);
           
            var containerNode = doc.DocumentNode.SelectSingleNode(ContainerNodeXPath);
            if (containerNode == null) return null;
      
            return new BookViewModel()
            {
                Origin = OriginName,
                OriginImageUrl = OriginImageUrl,
                Title = GetTitle(containerNode),
                ImageUrl = GetImageUrl(containerNode),
                Price = GetPrice(containerNode),
                BuyUrl = GetOriginUrl(containerNode),
            };
        }


        protected virtual string GetTitle(HtmlNode containerNode) 
        {
            var bookTitleNode = containerNode.SelectSingleNode(TitleXPath);
            return bookTitleNode?.InnerText ?? "Unknown Title";
        }
        protected virtual double? GetPrice(HtmlNode containerNode)
        {
            var priceNode = containerNode.SelectSingleNode(PriceXPath);
            if (priceNode == null) return null;

            var trimmedPrice = priceNode.InnerText.Trim();
            string price = Regex.Replace(trimmedPrice, @"[^\d.]", "");

            // Convert the cleaned text to a numeric type
            return double.TryParse(price, out double numericPrice) ? numericPrice : (double?)null;
        }

        protected virtual string GetImageUrl(HtmlNode containerNode)
        {
            var bookImg = containerNode.SelectSingleNode(ImageUrlXPath);
            string srcValue = bookImg?.GetAttributeValue(ImageUrlAttribute, "");
            if (string.IsNullOrEmpty(srcValue)) return NotFoundImage;

            return srcValue.StartsWith("https://") || srcValue.StartsWith("//www")
                ? srcValue
                : OriginBaseUrl + srcValue;
        }


        protected virtual string GetOriginUrl(HtmlNode containerNode)
        {
            var bookUrlNode = containerNode.SelectSingleNode(OriginUrlXPath);
            string srcValue = bookUrlNode?.GetAttributeValue(OriginUrlAttribute, "");
            if (string.IsNullOrEmpty(srcValue)) return NotFoundImage;

            return srcValue.StartsWith("https://") || srcValue.StartsWith("//www")
                ? srcValue
                : OriginBaseUrl + srcValue;
        }

    }
}