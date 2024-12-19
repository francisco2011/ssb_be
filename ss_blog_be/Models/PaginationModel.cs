namespace ss_blog_be.Models
{
    public class PaginationModel
    {
        // "page": 5,
      //"per_page": 20,
      //"page_count": 20,
      //"total_count": 521,
        public int TotalCount { get; set; } // total elements
        public int PageCount { get; set; } // total pages
        public int PageSize { get; set; } // elements per page
        public int Page { get; set; } // current page

        public PaginationModel(int count, int offSet, int totalElements)
        {
            TotalCount = totalElements;
            PageSize = count;
            Page = offSet == 0 || count == 0 ? 1 : ( offSet / count );
            PageCount = ( totalElements/count ) + (totalElements % count > 0 ? 1 : 0);
        }

        public PaginationModel(int count, int offSet)
        {
            TotalCount = 0;
            PageSize = count;
            Page = offSet == 0 || count == 0? 1 : offSet / count;
            PageCount = 0;
        }
    }
}
