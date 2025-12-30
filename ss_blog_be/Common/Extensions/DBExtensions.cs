namespace ss_blog_be.Common.Extensions
{
    public static class DBExtensions
    {
        public static bool IsNotNull(this object value)
        {
            return value != DBNull.Value && value != null;
        }
    }
}
