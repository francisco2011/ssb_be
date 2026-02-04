namespace ss_blog_be.Models
{
    public class PaginatedResult<T> where T : new()
    {
        public IEnumerable<T> Data { get; set; }
        public PaginationModel Pagination { get; set; }

        public PaginatedResult(IEnumerable<T> data, PaginationModel pagination)
        {
            Data = data;
            Pagination = pagination;
        }
    }
}
