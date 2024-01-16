using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace BookCompare.ScrapersNew
{
    public class DaniSfarim : Scraper
    {
        protected override string OriginName => "דני ספרים";
        protected override string OriginImageUrl => "~/Content/Images/DaniSfarim.jpg";
        protected override string OriginSearchUrl => "https://www.siman-kria.co.il/Search/?q=";

        protected override string ContainerNodeXPath => "//div[@id='SearchResultPage']//ul[@class='searchResultsList']";

        protected override string TitleXPath => "//div[contains(@class,'catalogItemBox')]//a[@class='item-text']//h3[@class='itemTitle']";

        protected override string PriceXPath => "//div[contains(@class,'catalogItemBox')]//a[@class='item-text']//span[@class='finalPrice']";

        protected override string ImageUrlXPath => "//div[contains(@class,'catalogItemBox')]//a[@class='item-image ']//img";

        protected override string ImageUrlAttribute => "src";

        protected override string OriginUrlXPath => "//div[contains(@class,'catalogItemBox')]//a[@class='item-text']";

        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://www.siman-kria.co.il/";



    }

}