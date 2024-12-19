using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ss_blog_be.Common.Extensions
{
    public static class Base64Ext
    {
        public static string ToBase64(this string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return WebEncoders.Base64UrlEncode(bytes);
        }

        public static string FromBase64(this string str)
        {
            byte[] bytes = WebEncoders.Base64UrlDecode(str);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
