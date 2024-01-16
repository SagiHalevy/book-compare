using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookCompare.ScrapersNew
{
    public class Sipurpashut : Scraper
    {
        protected override string OriginName => "סיפור פשוט";

        protected override string OriginImageUrl => "~/Content/Images/Sipurpashut.jpg";

        protected override string OriginSearchUrl => "https://www.sipurpashut.com/search?q=";

        protected override string ContainerNodeXPath => "//div[contains(@class,'search-results')]//div[contains(@class,'block')]";

        protected override string TitleXPath => "//div[@class='sub']//a[@class='product-block-title']";

        protected override string PriceXPath => "//div[@class='sub']//div[@class='pricearea']//span";

        protected override string ImageUrlXPath => "//div[@class='main']//div[@class='product-image ']//div";

        protected override string ImageUrlAttribute => "data-bgset";

        protected override string OriginUrlXPath => "//div[@class='sub']//a[@class='product-block-title']";

        protected override string OriginUrlAttribute => "href";

        protected override string OriginBaseUrl => "https://www.sipurpashut.com/";


    }
}