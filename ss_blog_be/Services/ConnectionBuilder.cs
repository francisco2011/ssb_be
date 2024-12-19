using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.Dapper;

namespace ss_blog_be.Services
{
    public class ConnectionBuilder
    {
        public SqliteConnection Connect()
        {

            string databaseName = "./Db/blog_v2.db";
            return new SqliteConnection("Data Source=" + databaseName);
        }


    }
}
