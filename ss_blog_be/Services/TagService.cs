using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using System.ComponentModel.DataAnnotations;
using ss_blog_be.Models;
using ss_blog_be.Common.Extensions;

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

        public async Task Delete(long id)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                    .From("post")
                    .Select("ROWID", "id")
                    .Select("isPublished")
                    .Select("tags")
                    .Select("tagsCodeSnippets")
                    .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                    .From("postType", "type")
                    .Select("ROWID", "typeId")
                    .Select("name")
                    .Join("post", "postType", "typeId", "ROWID", SQLBuilderJoinTypeEnum.LEFT)
                    .From("postFTS")
                    .Select("ROWID", "postfts_rowid")
                    .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                    .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

            if (dyna == null) throw new Exception("Not found");
            if (Convert.IsDBNull(dyna.postfts_rowid) == null) return;

            string _sql = string.Empty;
            string __sql = string.Empty;


            if (dyna.typeId == 5)
            {
                //nothing to do here 
                if (string.IsNullOrEmpty(dyna.tagsCodeSnippets)) return;

                _sql = $"UPDATE post SET tagsCodeSnippets = '', previousTags = '{dyna.tagsCodeSnippets}' WHERE ROWID = {id}";

                //INSERT INTO ft(ft, rowid, a, b, c) VALUES('delete', 14, $a, $b, $c);
                __sql = $"INSERT INTO postFTS (postFTS, rowid, tags, tagsCodeSnippets) VALUES ('delete', '{id}', NULL ,'{dyna.tagsCodeSnippets}')";
            }
            else
            {
                //nothing to do here 
                if (string.IsNullOrEmpty(dyna.tags)) return;
                _sql = $"UPDATE post SET tags = '',  previousTags = '{dyna.tags}' WHERE ROWID = {id}";
                __sql = $"INSERT INTO postFTS (postFTS, rowid, tags, tagsCodeSnippets) VALUES ('delete', '{id}', '{dyna.tags}', NULL)";
            }

            var result = await this._conn.ExecuteAsync(_sql);
            var _result = await this._conn.ExecuteAsync(__sql);
            await Rebuild();

            return;
        }

        public async Task Restore(long id)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                    .From("post")
                    .Select("ROWID", "id")
                    .Select("isPublished")
                    .Select("previousTags")
                    .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                    .From("postType", "type")
                    .Select("ROWID", "typeId")
                    .Select("name")
                    .Join("post", "postType", "typeId", "ROWID", SQLBuilderJoinTypeEnum.LEFT)
                    .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

            if (dyna == null) throw new Exception("Not found");
            if (Convert.IsDBNull(dyna.typeId)) throw new Exception("Can not set the tags for a Post withtout type");

            string _sql = string.Empty;
            string __sql = string.Empty;

            if (string.IsNullOrEmpty(dyna.previousTags)) return;

            if (dyna.typeId == 5)
            {
                _sql = $"UPDATE post SET tagsCodeSnippets = '{dyna.previousTags}' WHERE ROWID = {id}";
                __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', NULL ,'{dyna.previousTags}') Returning RowId";
            }
            else
            {
                _sql = $"UPDATE post SET tags = '{dyna.previousTags}' WHERE ROWID = {id}";
                __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', '{dyna.previousTags}', NULL) Returning RowId";
            }

            var result = await this._conn.ExecuteAsync(_sql);
            var _result = await this._conn.ExecuteAsync(__sql);
            
            await Rebuild();
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
                        .Select("ROWID", "postfts_rowid")
                        .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                        .Build();

                var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

                if (dyna == null) throw new Exception("Not found");
                if (Convert.IsDBNull(dyna.typeId)) throw new Exception("Can not set the tags for a Post withtout type");

                var contentAsStr = string.Join(" ", tags);
                string _sql = string.Empty;
                string __sql = string.Empty;


                if (dyna.typeId == 5)
                {
                    //nothing to do here 
                    if (!Convert.IsDBNull(dyna.tagsCodeSnippets) && contentAsStr == dyna.tagsCodeSnippets) return;
                    _sql = $"UPDATE post SET tagsCodeSnippets = '{contentAsStr}' WHERE ROWID = {id}";
                    __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', NULL ,'{contentAsStr}') Returning RowId";
                }
                else
                {
                    //nothing to do here 
                    if (!Convert.IsDBNull(dyna.tags) && contentAsStr == dyna.tags) return;
                    _sql = $"UPDATE post SET tags = '{contentAsStr}' WHERE ROWID = {id}";
                    __sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{id}', '{contentAsStr}', NULL) Returning RowId";
                }

                var result = await this._conn.ExecuteAsync(_sql);
                var _result = await this._conn.ExecuteAsync(__sql);
                await Rebuild();

            }
        }

        public async Task<IEnumerable<TagModel>> GetTags(int? postTypeId)
        {
            try
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
            catch (Exception ex)
            {
                throw;
            }
            
        }
    }
    }
