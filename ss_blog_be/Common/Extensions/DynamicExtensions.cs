using System.Text.Json;

namespace ss_blog_be.Common.Extensions
{
    public static class DynamicExtensions
    {
        public static bool HasProperty(dynamic item, string propertyName)
            => (item as IDictionary<string, object>).ContainsKey(propertyName);

        public static DateTime GetAsDateTime(dynamic item, string property, DateTime defaultValue)
        {
            var asLong = GetPropertyValueAs<long>(item, property, 0);

            if(asLong == 0) return defaultValue;

            return new DateTime(asLong);
        }

        public static T GetPropertyValueAs<T>(dynamic item, string property, T defaultValue)
        {

            if(item == null) throw new Exception("item can not be null");

            var asDic = item as IDictionary<string, object>;

            if (asDic == null) throw new Exception("item can not be used as IDictionary<string, object>");

            if (asDic.ContainsKey(property))
            {
                var val = asDic[property];

                if(val is T) return (T)val;
            }

            return defaultValue;

        }
    }
}
