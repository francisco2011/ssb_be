namespace ss_blog_be.Common.Extensions
{
    public static class DynamicExtensions
    {
        public static bool HasProperty(dynamic item, string propertyName)
            => (item as IDictionary<string, object>).ContainsKey(propertyName);
    }
}
