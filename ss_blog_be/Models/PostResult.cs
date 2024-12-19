namespace ss_blog_be.Models
{
    public class PostResult
    {
        public IEnumerable<PostModel> Posts { get; set; }
        public PaginationModel Pagination { get; set; }

        public PostResult(IEnumerable<PostModel> posts, PaginationModel pagination)
        {
            Posts = posts;
            Pagination = pagination;
        }
    }
}
