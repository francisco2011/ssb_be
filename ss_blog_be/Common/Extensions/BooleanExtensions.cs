namespace ss_blog_be.Common.Extensions
{
    public static class BooleanExtensions
    {
            public static int ToInt(this bool value)
            {
                return value ? 1 : 0;
            }

        public static bool ToBool(this long value)
        {
            return value == 1 ? true : false;
        }

        public static bool ToBool(this int value)
        {
            return value == 1? true : false;
        }

    }
}
