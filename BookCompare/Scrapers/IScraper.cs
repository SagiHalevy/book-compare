using BookCompare.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCompare.Scrapers
{
    internal interface IScraper
    {
        BookViewModel GetBookData(string bookName);

    }
}
