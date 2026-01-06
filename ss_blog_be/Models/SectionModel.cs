using System.Text.Json.Serialization;

namespace ss_blog_be.Models
{
    public class SectionModel
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Tag { get; set; }

        [JsonIgnore]
        public long createdAtTicks { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool Modifiable { get; set; }

        public SectionModel setCreatedAt()
        {
            CreatedAt = new DateTime(createdAtTicks);
            return this;
        }
    }

    public class SectionResult
    {
        public IEnumerable<SectionModel> Sections { get; set; }
        public PaginationModel Pagination { get; set; }

        public SectionResult(IEnumerable<SectionModel> sections, PaginationModel pagination)
        {
            Sections = sections;
            Pagination = pagination;
        }
    }
}
