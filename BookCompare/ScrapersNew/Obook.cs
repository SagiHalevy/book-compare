using BookCompare.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace BookCompare.ScrapersNew
{
    public class Obook : Scraper
    {
        static HtmlDocument htmlDocument = new HtmlDocument();
        protected override string OriginName => "Obook";

        protected override string OriginImageUrl => "~/Content/Images/Obook.jpg";

        protected override string OriginSearchUrl => "https://www.obook.co.il/10/%D7%97%D7%99%D7%A4%D7%95%D7%A9-%D7%A1%D7%A4%D7%A8%D7%99%D7%9D";

        protected override string ContainerNodeXPath => "//div[@class='itxtprod']//div[@class='prodbox']";

        protected override string TitleXPath => ".//a//h4";

        protected override string PriceXPath => ".//a//p/strike/following-sibling::text()[1]";

        protected override string ImageUrlXPath => ".//a//div[@class='imgbox']/img";

        protected override string ImageUrlAttribute => "src";

        protected override string OriginUrlXPath => ".//a";

        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://www.obook.co.il/";


        //override because searching a book is done with POST request
        public override async Task<BookViewModel> GetScrapedDataAsync(string bookName, CancellationToken cancellationToken)
        {

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    string url = OriginSearchUrl;
                    var encodedBookName = Uri.EscapeDataString(bookName);

                    Dictionary<string, string> formData = new Dictionary<string, string>
                    {
                        { "ser", bookName }
                    };

                    FormUrlEncodedContent content = new FormUrlEncodedContent(formData);

                    // Check if the cancellation token has been triggered before making the web request
                    cancellationToken.ThrowIfCancellationRequested();

                    HttpResponseMessage response = await httpClient.PostAsync(url, content, cancellationToken);

                    if (!response.IsSuccessStatusCode) return null;

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseContent)) return null;

                    htmlDocument.LoadHtml(responseContent);
                    
                    var containerNode = htmlDocument.DocumentNode.SelectSingleNode(ContainerNodeXPath);
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
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"HTTP request exception: {ex.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return null;
                }
            }
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