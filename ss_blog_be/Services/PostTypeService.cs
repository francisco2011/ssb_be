using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;

namespace ss_blog_be.Services
{
    public class PostTypeService
    {
        private SqliteConnection _conn { get; }

        public PostTypeService([Required] SqliteConnection conn)
        {
            _conn = conn;
        }

        public async Task<IEnumerable<PostTypeModel>> Get()
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("postType")
                        .Select("name", "name")
                        .Select("ROWID", "id")
                        .Build();


            var result = await this._conn.QueryAsync<PostTypeModel>(sql);

            return result;
        }
    }
}
