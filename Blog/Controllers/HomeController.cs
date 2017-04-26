using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var db = new BlogDbContext();

            var articles = db.Articles
                .OrderByDescending(a => a.Id)
                .Take(3)
                .Select(a => new ArticleDetailsModel
                {
                    Id = a.Id,
                    ImagePath = a.ImagePath,
                    Title = a.Title,
                    Content = a.Content
                })
            .ToList();

            return View(articles);
        }
    }
}