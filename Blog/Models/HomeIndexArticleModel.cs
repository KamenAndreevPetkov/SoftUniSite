namespace Blog.Models
{
    public class HomeIndexArticleModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImagePath { get; set; }

        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public bool IsAuthor(string authorId)
        {
            return AuthorId == authorId;
        }
    }
}