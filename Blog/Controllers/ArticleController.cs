using System.Web.Mvc;
namespace Blog.Controllers
{
    using Blog.Models;
    using Microsoft.AspNet.Identity;
    using System.Web.Mvc;
    using System.Data.Entity;
    using System.Linq;
    using System.Web;
    using System.IO;
    using System;
    using System.Text;

    public class ArticleController : Controller
    {
        public ActionResult List(string user = null)
        {
            using (var db = new BlogDbContext())
            {
                var articles = db.Articles
                    .OrderByDescending(a => a.Id)
                    .Include(a => a.Author)
                    .ToList();

                if (user != null)
                {
                   articles = articles
                        .Where(a => a.Author.Email == user).ToList();
                }

                return View(articles);
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Article model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var authorId = User.Identity.GetUserId();

                    model.AuthorId = authorId;

                    if (image != null)
                    {
                        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

                        if (allowedContentTypes.Contains(image.ContentType))
                        {
                            var imagesPath = "/Content/Images/";

                            var filename = ConvertFilenameToRandomizedFilename(image.FileName);

                            var uploadPath = imagesPath + filename;

                            image.SaveAs(Server.MapPath(uploadPath));

                            model.ImagePath = uploadPath;
                        }
                    }

                    db.Articles.Add(model);
                    db.SaveChanges();
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Details(int id)
        {
            var db = new BlogDbContext();

            var article = db.Articles
                .Where(a => a.Id == id)
                .Include(a => a.Comments)
                .Include(a => a.Author)
                .Select(a => new ArticleDetailsModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    ImagePath = a.ImagePath,
                    Author = a.Author,
                    Comments = a.Comments
                }).FirstOrDefault();

            if (article == null)
            {
                return HttpNotFound();
            }

            return View(article);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Find(id);

                if (article == null || !IsAuthorized(article))
                {
                    return HttpNotFound();
                }

                var articleViewModel = new ArticleViewModel
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    ImagePath = article.ImagePath,
                    AuthorId = article.AuthorId
                };

                return View(articleViewModel);
            }
        }

        [Authorize]
        [ActionName("Delete")]
        [HttpPost]
        public ActionResult ConfirmDelete(int id)
        {
            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Find(id);

                if (article == null || !IsAuthorized(article))
                {
                    return HttpNotFound();
                }

                var oldImagePath = article.ImagePath;
                if (oldImagePath != null)
                {
                    var absoluteOldImagePath = Server.MapPath(oldImagePath);

                    System.IO.File.Delete(absoluteOldImagePath);
                }

                db.Articles.Remove(article);
                db.SaveChanges();

                return RedirectToAction("List");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var db = new BlogDbContext())
            {
                var article = db.Articles.Find(id);

                if (article == null || !IsAuthorized(article))
                {
                    return HttpNotFound();
                }

                var articleViewModel = new ArticleViewModel
                {
                    Id = article.Id,
                    Title = article.Title,
                    Content = article.Content,
                    ImagePath = article.ImagePath,
                    AuthorId = article.AuthorId
                };

                db.SaveChanges();

                return View(articleViewModel);
            }

        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                using (var db = new BlogDbContext())
                {
                    var article = db.Articles.Find(model.Id);

                    if (article == null || !IsAuthorized(article))
                    {
                        return HttpNotFound();
                    }

                    article.Title = model.Title;
                    article.Content = model.Content;

                    if (image != null)
                    {
                        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

                        if (allowedContentTypes.Contains(image.ContentType))
                        {
                            var imagesPath = "/Content/Images/";

                            var filename = ConvertFilenameToRandomizedFilename(image.FileName);

                            var uploadPath = imagesPath + filename;

                            image.SaveAs(Server.MapPath(uploadPath));

                            // deleting the old image
                            var oldImagePath = article.ImagePath;

                            article.ImagePath = uploadPath;

                            if (oldImagePath != null)
                            {
                                var absoluteOldImagePath = Server.MapPath(oldImagePath);

                                System.IO.File.Delete(absoluteOldImagePath);
                            }
                        }
                    }
                    else
                    {
                        var oldImagePath = article.ImagePath;
                        if (oldImagePath != null)
                        {
                            var absoluteOldImagePath = Server.MapPath(oldImagePath);

                            System.IO.File.Delete(absoluteOldImagePath);

                            article.ImagePath = null;
                        }
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("Details", new { id = model.Id });
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult PostComment(int? id, CommentViewModel commentViewModel)
        {
            var db = new BlogDbContext();
            if (id == null)
            {
                return HttpNotFound("No article found");
            }

            var article = db.Articles.Include(a => a.Comments).FirstOrDefault(a => a.Id == id);

            var commentAuthorId = User.Identity.GetUserId();

            var comment = new Comment()
            {
                Subject = commentViewModel.Subject,
                Content = commentViewModel.Content,
                Date = DateTime.Now,
                AuthorId = commentAuthorId,
                ArticleId = article.Id
            };

            article.Comments.Add(comment);

            db.SaveChanges();

            return RedirectToAction("Details", new { Id = article.Id });
        }

        private string ConvertFilenameToRandomizedFilename(string filename)
        {
            var imageFilenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            var imageExtension = Path.GetExtension(filename);

            var randomChars = GetRandomDigitLowercaseUppercaseChars(5);

            var imageNewName = $"{imageFilenameWithoutExtension}_{randomChars}{imageExtension}";

            return imageNewName;
        }

        private string GetRandomDigitLowercaseUppercaseChars(int count)
        {
            var digits = Enumerable.Range('0', 10).Select(a => (char)a);
            var uppercaseLetters = Enumerable.Range('A', 26).Select(a => (char)a);
            var lowercaseLetters = Enumerable.Range('a', 26).Select(a => (char)a);

            var allChars = digits.Concat(uppercaseLetters).Concat(lowercaseLetters).ToArray();
            var allCharsLength = allChars.Length;

            var random = new Random();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(allChars[random.Next(0, allCharsLength - 1)]);
            }

            return result.ToString();
        }

        public bool IsAuthorized(Article article)
        {
            var isAdmin = User.IsInRole("Admin");
            var isAuthor = article.IsAuthor(User.Identity.GetUserId());

            return isAdmin || isAuthor;
        }
    }
}