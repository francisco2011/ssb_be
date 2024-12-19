using Dapper;
using System.Data;

namespace ss_blog_be.Common.Dapper
{
    public class CustomDatetimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        private readonly TimeZoneInfo databaseTimeZone = TimeZoneInfo.Local;
        public static readonly CustomDatetimeHandler Default = new CustomDatetimeHandler();

        public CustomDatetimeHandler()
        {

        }


        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value.Ticks;
        }

        public override DateTime Parse(object value)
        {
            var val = (long)value;

            return new DateTime(val);
        }
    }
}
