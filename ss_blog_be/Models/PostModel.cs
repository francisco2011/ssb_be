using ss_blog_be.Common.Extensions;
using System.Text.Json.Serialization;

namespace ss_blog_be.Models
{
    public class PostModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public PostTypeModel Type { get; set; }
        public int? Id { get; set; }

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

        public static PostModel From(dynamic dym)
        {
            var model = new PostModel();

            string name = DynamicExtensions.GetPropertyValueAs<string>(dym, "name", string.Empty);
            string titleOriginal = DynamicExtensions.GetPropertyValueAs<string>(dym, "title", string.Empty);
            string contentOriginal = DynamicExtensions.GetPropertyValueAs<string>(dym, "content", string.Empty);
            string descriptionOriginal = DynamicExtensions.GetPropertyValueAs<string>(dym, "description", string.Empty);
            var id = Convert.ToInt32(DynamicExtensions.GetPropertyValueAs<long>(dym, "id", 0));
            long _isPublished = DynamicExtensions.GetPropertyValueAs<long>(dym, "isPublished", 0);
            var isPublished = _isPublished.ToBool();
            var createdAt = DynamicExtensions.GetAsDateTime(dym, "createdAtTicks", new DateTime());
            var typeId = Convert.ToInt32(DynamicExtensions.GetPropertyValueAs<long>(dym, "typeId", 0));
            string typeName = DynamicExtensions.GetPropertyValueAs<string>(dym, "typeName", string.Empty);

            string tags = DynamicExtensions.GetPropertyValueAs<string>(dym, "tags", string.Empty);

            return new PostModel
            {
                Id = id,
                Name = name,
                Content = contentOriginal.FromBase64(),
                CreatedAt = createdAt,
                Description = descriptionOriginal.FromBase64(),
                Title = titleOriginal.FromBase64(),
                IsPublished = isPublished,
                Type = new PostTypeModel()
                {
                    Id = typeId,
                    Name = typeName,
                },
                Tags = string.IsNullOrEmpty(tags)? [] : tags.Split(" "),
                Contents = new List<ContentModel>(),
                
            };
        }
    }


}
