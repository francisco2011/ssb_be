using System.Text.Json.Serialization;

namespace ss_blog_be.Types
{
    [JsonConverter(typeof(JsonStringEnumConverter<ContentType>))]
    public enum ContentType
    {
        preview = 0, imgBody = 1
    }
}
