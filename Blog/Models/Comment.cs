using System;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Subject { get; set; }
        
        public string Content { get; set; }

        public int ArticleId { get; set; }

        public virtual Article Article { get; set; }

        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public DateTime Date { get; set; }
    }
}