using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace ss_blog_be.Storage
{
    public class StorageSettings
    {
        public string StorageUrl { get; set; }
        public string PublicUrl { get; set; }
        public string StorageAccessKey { get; set; }
        public string StorageAccessKeyId { get; set; }
        public string MainBucket { get; set; }

        public StorageSettings() { }
    }
}
