namespace ss_blog_be.Models.Storage
{
    public class StorageObjectModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public long? Size { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public StorageObjectType Type { get; set; }

    }
}
