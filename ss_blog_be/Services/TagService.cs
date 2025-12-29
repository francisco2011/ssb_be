using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System;
using System.Security.Cryptography;
using ss_blog_be.Models;

namespace ss_blog_be.Services
{
    public class TagService
    {
        private SqliteConnection _conn { get; }
        private StorageService _storageService { get; }

        public TagService([Required] SqliteConnection conn)
        {
            _conn = conn;

        }

        public async Task Rebuild()
        {
            await this._conn.ExecuteAsync("INSERT INTO postFTS(postFTS) VALUES('rebuild');");
        }

        public async Task UpdateTags(long id, ICollection<string> tags)
        {
            if (tags != null && tags.Any())
            {

                var sqlBuilder = new SQLBuilderS();
                var sql = sqlBuilder.Init()
                        .From("post")
                        .Select("ROWID", "id")
                        .Select("isPublished")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                        .From("postType", "type")
                        .Select("ROWID", "typeId")
                        .Select("name")
                        .Join("post", "postType", "typeId", "ROWID", SQLBuilderJoinTypeEnum.LEFT)
                        .From("postFTS")
                        .Select("tags")
                        .Select("tagsCodeSnippets")
                        .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                        .Build();

                var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

                if (dyna == null) throw new Exception("Not found");

                var contentAsStr = string.Join(" ", tags);
                string _sql = string.Empty;
                string __sql = string.Empty;


                if (dyna.typeId == 5)
                {
                    //nothing to do here 
                    if (contentAsStr == dyna.tagsCodeSnippets) return;
                    _sql = $"UPDATE post SET tagsCodeSnippets = '{contentAsStr}' WHERE ROWID = {id}";
                    __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', NULL ,'{contentAsStr}') Returning RowId";
                }
                else
                {
                    //nothing to do here 
                    if (contentAsStr == dyna.tags) return;
                    _sql = $"UPDATE post SET tags = '{contentAsStr}' WHERE ROWID = {id}";
                    __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', '{contentAsStr}', NULL) Returning RowId";
                }

                var result = await this._conn.ExecuteAsync(_sql);
                var _result = await this._conn.ExecuteAsync(__sql);
                await Rebuild();

                return;
            }
        }

        public async Task<IEnumerable<TagModel>> GetTags(int? postTypeId)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("postFTS_v")
                        .Select("term", "term")
                        .Select("cnt", "ocurrences");

            if (postTypeId.HasValue)
            {
                if (postTypeId.Value == 5)
                {
                    sql.Where("col", SQLBuilderOperatorsEnum.EQUAL, "'tagsCodeSnippets'");
                }
                else
                {
                    sql.Where("col", SQLBuilderOperatorsEnum.EQUAL, "'tags'");
                }
            }


            var query = sql.Build();
            var result = await this._conn.QueryAsync<TagModel>(query);

            return result.OrderByDescending(c => c.Ocurrences);
        }
    }
    }
