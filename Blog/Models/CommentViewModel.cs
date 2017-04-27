namespace Blog.Models
{
    public class CommentViewModel
    {
        public CommentViewModel()
        {
        }

        public CommentViewModel(int articleId)
        {
            this.ArticleId = articleId;
        }

        public int ArticleId { get; set; }
        
        public string Subject { get; set; }
        
        public string Content { get; set; }
    }
}