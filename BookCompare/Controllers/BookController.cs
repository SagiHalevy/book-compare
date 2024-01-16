using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BookCompare.DataAccess;
using BookCompare.ScrapersNew;
using BookCompare.Services;
using BookCompare.ViewModels;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BookCompare.Controllers
{
    public class BookController : Controller
    {

        static BooksService booksService = new BooksService(); 

        // GET: Book
        public ActionResult Index()
        {
            return View();
        }

        // Get: Book

        public async Task<ActionResult> Search(string bookName)
        {
            var sortedBookViewModels = await booksService.GetAllBooks(bookName);
            ViewBag.SearchInput = bookName;
            return View("Book", sortedBookViewModels);
        }



        // GET: Book
        public ActionResult Book()
        {
            return View();
        }
    }
}