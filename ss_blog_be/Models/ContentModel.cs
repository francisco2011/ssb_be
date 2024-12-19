using ss_blog_be.Types;

namespace ss_blog_be.Models
{
    public class ContentModel
    {
        public string Name { get; set; }
        public ContentType Type { get; set; }
        public string? MimeType { get; set; }
        public string? Url { get; set; }
    }
}
