namespace ss_blog_be.Models
{
    public class PostResult
    {
        public IEnumerable<PostModel> Posts { get; set; }
        public int TotalElements { get; set; }

        public PostResult(IEnumerable<PostModel> posts, int totalElements)
        {
            Posts = posts;
            TotalElements = totalElements;
        }
    }
}
