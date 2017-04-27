namespace Blog.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Article
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Title { get; set; }
        
        [Required]
        public string Content { get; set; }

        public string ImagePath { get; set; }

        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public int CommentId { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string Comment { get; set; }

        public bool IsAuthor(string authorId)
        {
            return AuthorId == authorId;
        }
    }
}