using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using System.ComponentModel.DataAnnotations;
using ss_blog_be.Models;
using ss_blog_be.Storage;
using ss_blog_be.Common.Extensions;
using Amazon.S3.Model;

namespace ss_blog_be.Services.Data
{
    public class TagDataService
    {
        private SqliteConnection _conn { get; }
        private StorageService _storageService { get; }

        public TagDataService([Required] SqliteConnection conn)
        {
            _conn = conn;

        }

        public async Task Delete(int postId, int postTypeId, bool isPhysicalDelete = false)
        {
            if (postTypeId == default) return;

            var postFtsTableName = getFTSTableName(postTypeId);

            var dyna = await getTagsAndFtsFor(postId, postTypeId);

            if (dyna == null || dyna.Id == -1) return;

            string _sql = string.Empty;
            string __sql = string.Empty;

            if (string.IsNullOrEmpty(dyna.Content)) return;

            _sql = isPhysicalDelete ? $"DELETE from tags WHERE ROWID = {dyna.Id}" :
                                        $"UPDATE tags SET content = '', previousContent = '{dyna.Content}' WHERE ROWID = {dyna.Id}";
            __sql = $"INSERT INTO {postFtsTableName} ({postFtsTableName}, rowid, content) VALUES ('delete', '{dyna.Id}')";


            var result = await _conn.ExecuteAsync(_sql);
            var _result = await _conn.ExecuteAsync(__sql);
            await Rebuild(postTypeId);

            return;
        }

        public async Task Restore(int postId, int postTypeId)
        {
            var postFtsTableName = getFTSTableName(postTypeId);

            var dyna = await getTagsAndFtsFor(postId, postTypeId);

            if (dyna == null || string.IsNullOrEmpty(dyna.PreviousContent)) return;

            string _sql = string.Empty;
            string __sql = string.Empty;

            _sql = $"INSERT OR REPLACE tags SET content = '{dyna.PreviousContent}' WHERE ROWID = {dyna.Id}";
            __sql = $"INSERT OR REPLACE INTO {postFtsTableName}(rowid, content, previousContent) VALUES ('{dyna.Id}', '{dyna.PreviousContent}' ,'') Returning RowId";

            var result = await _conn.ExecuteAsync(_sql);
            var _result = await _conn.ExecuteAsync(__sql);

            await Rebuild(postTypeId);
        }

        public async Task Rebuild(int postTypeId)
        {
            var postFtsTableName = getFTSTableName(postTypeId);
            await _conn.ExecuteAsync($"INSERT INTO {postFtsTableName}({postFtsTableName}) VALUES('rebuild');");
        }

        public async Task<TagModel> getTagsAndFtsFor(int postId, int postTypeId)
        {
            var postFtsTableName = getFTSTableName(postTypeId);

            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                    .From("tags")
                    .Select("ROWID", "id")
                    .Select("content", "tagsContent")
                    .Select("previousContent", "previousContent")
                    .Where("postId", SQLBuilderOperatorsEnum.EQUAL, postId)
                    .Join("tags", postFtsTableName, "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                    .From(postFtsTableName)
                    .Select("content")
                    .Select("ROWID", "postfts_rowid")
                    .Build();

            var dyna = await _conn.QueryFirstOrDefaultAsync(sql);

            if (dyna == null) return null;

            int id = Convert.ToInt32(DynamicExtensions.GetPropertyValueAs<long>(dyna, "id", 0));
            string tagsContent = DynamicExtensions.GetPropertyValueAs<string>(dyna, "tagsContent", string.Empty);
            string previousContent = DynamicExtensions.GetPropertyValueAs<string>(dyna, "previousContent", string.Empty);
            int ftsId = Convert.ToInt32(DynamicExtensions.GetPropertyValueAs<long>(dyna, "postfts_rowid", 0));

            return new TagModel { Content = tagsContent, PreviousContent = previousContent, Id = id, FtsId = ftsId };
        }


        public async Task UpdateTags(int postId, int postTypeId, ICollection<string> tags)
        {
            if (tags != null && tags.Any())
            {
                var postFtsTableName = getFTSTableName(postTypeId);

                var dyna = await getTagsAndFtsFor(postId, postTypeId);

                var contentAsStr = string.Join(" ", tags);
                string _sql = string.Empty;
                string __sql = string.Empty;

                if (dyna != null && !string.IsNullOrEmpty(dyna.Content) && contentAsStr == dyna.Content) return;

                _sql = dyna == null ? $"INSERT into tags (content, previousContent, postId) values('{contentAsStr}', '', {postId}) RETURNING rowid" :
                    $"UPDATE tags set content = '{contentAsStr}', previousContent = '' where ROWID = {dyna.Id} RETURNING rowid";

                var result = await _conn.ExecuteAsync(_sql);

                __sql = $"INSERT OR REPLACE INTO {postFtsTableName} (ROWID, content) VALUES ('{result}', '{contentAsStr}') Returning RowId";

                var _result = await _conn.ExecuteAsync(__sql);
                await Rebuild(postTypeId);

            }
        }

        private string getFTSTableName(int postTypeId)
        {
            return "postFTS_" + postTypeId;
        }

        private string getFTSVTableName(int postTypeId)
        {
            return "postFTS_v_" + postTypeId;
        }

        public async Task<TagOcurrencesModel[]> GetTags(int postTypeId)
        {
            try
            {

                var sqlBuilder = new SQLBuilderS();
                var sql = sqlBuilder.Init()
                            .From(getFTSVTableName(postTypeId))
                            .Select("term", "term")
                            .Select("cnt", "ocurrences");

                var query = sql.Build();
                var result = await _conn.QueryAsync<TagOcurrencesModel>(query);

                return result.OrderByDescending(c => c.Ocurrences).ToArray();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
