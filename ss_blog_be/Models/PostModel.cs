using System.Text.Json.Serialization;

namespace ss_blog_be.Models
{
    public class PostModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public PostTypeModel Type { get; set; }
        public long? Id { get; set; }

        [JsonIgnore]
        public long createdAtTicks { get; set; }
        public DateTime? CreatedAt { get; set; }
        public ICollection<string> Tags { get; set; }
        public ICollection<ContentModel> Contents { get; set; }
        public bool IsPublished { get; set; }

        public PostModel setCreatedAt()
        {
            CreatedAt = new DateTime(createdAtTicks);
            return this;
        }
    }

    public class TagsModel
    {
        public string[] Content { get; set; }
    }
}
