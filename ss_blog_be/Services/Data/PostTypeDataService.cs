using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;
using ss_blog_be.Common.Extensions;
using System.Xml.Linq;
using System;
using System.Net.NetworkInformation;
using System.Linq;

namespace ss_blog_be.Services.Data
{
    public class PostTypeDataService
    {
        private SqliteConnection _conn { get; }

        public PostTypeDataService([Required] SqliteConnection conn)
        {
            _conn = conn;
        }

        public async Task Update(PostTypeModel model)
        {
            string sql = $"UPDATE postType  set name =  '{model.Name}'";
            await _conn.ExecuteAsync(sql);
        }

        public async Task<PostTypeModel> Save(PostTypeModel model)
        {
            //INSERT INTO postType (name, ROWID) VALUES ('Code Snippet', 5);
            //CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_1 USING fts5(content, content=tags, content_rowid=ROWID);
            //CREATE VIRTUAL TABLE IF NOT EXISTS postFTS_v_1 USING fts5vocab(postFTS_1, col);

            string sql = $"INSERT INTO postType (name) VALUES ('{model.Name}') Returning RowId";
            var id = await _conn.ExecuteScalarAsync<int>(sql, model);

            var postFTSTableName = "postFTS_" + id;
            var postFTSVTableName = "postFTS_v_" + id;

            var postFTSSql = $"CREATE VIRTUAL TABLE IF NOT EXISTS {postFTSTableName} USING fts5(content, content=tags, content_rowid=ROWID);";
            var postFTSVSql = $"CREATE VIRTUAL TABLE IF NOT EXISTS {postFTSVTableName} USING fts5vocab({postFTSTableName}, col);";

            await _conn.ExecuteAsync(postFTSSql);
            await _conn.ExecuteAsync(postFTSVSql);

            model.Id = id;
            return model;
        }

        public async Task<IEnumerable<PostTypeModel>> Get()
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("postType")
                        .Select("name", "name")
                        .Select("ROWID", "id")
                        .Build();


            var result = await _conn.QueryAsync<PostTypeModel>(sql);

            return result;
        }

        public async Task<PostTypeModel> Get(int? id, string? name)
        {
            var sqlBuilder = new SQLBuilderS();
            var builder = sqlBuilder.Init()
                        .From("postType")
                        .Select("name", "name")
                        .Select("ROWID", "id");

            if (id.HasValue)
            {
                builder.Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                builder.Where("name", SQLBuilderOperatorsEnum.EQUAL, name);
            }

            var sql = builder.Build();

            var result = await _conn.QueryFirstOrDefaultAsync<PostTypeModel>(sql);

            return result;
        }

        private async Task<PaginationModel> Count(int count, int offset)
        {

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("postType", "pst")
                        .Select("1", "totalElements", SQLBuilderFunctions.COUNT);

            var sql = q.Build();
            var totalElements = await _conn.QuerySingleAsync<int>(sql);
            return new PaginationModel(count, offset, totalElements);
        }

        public async Task<PaginatedResult<PostTypeModel>> List(int count, int offset)
        {
            var pag = new PaginationModel(count, offset);

            if (count != 0)
            {
                pag = await Count(count, offset);

                if (pag.TotalCount == 0) return new PaginatedResult<PostTypeModel>([], pag);
            }

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init().From("postType")
                        .Select("name", "name")
                        .Select("ROWID", "id")
                        .Limit(count)
                        .Offset(offset);


            var sql = q.Build();

            var dyna = await _conn.QueryAsync<PostTypeModel>(sql);

            if (dyna == null) return new PaginatedResult<PostTypeModel>([], pag);

            return new PaginatedResult<PostTypeModel>(dyna, pag);
        }
    }
}
