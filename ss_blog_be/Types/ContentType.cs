using System.Text.Json.Serialization;

namespace ss_blog_be.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter<ContentType>))]
    public enum ContentType
    {
        preview = 0, 
        imgBody = 1,
        render = 2,
        titleRender = 3,
        descriptionRender = 4,
    }
}
