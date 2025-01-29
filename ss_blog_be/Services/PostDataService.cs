using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;
using Dapper;
using ss_blog_be.Common.SQLBuilder;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Types;
using ss_blog_be.Common.Extensions;
using System.Reflection;
using System.Data.SqlTypes;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Net.Http.Headers;
using Amazon.S3.Model;
[module: DapperAot]

namespace ss_blog_be.Services
{
    public class PostDataService
    {
        private SqliteConnection _conn { get; }
        private StorageService _storageService { get; }

        public PostDataService([Required] SqliteConnection conn, StorageService storageService) 
        {
            _conn = conn;
            _storageService = storageService;

        }

        public async Task<ContentModel> UpdateContent(int id, Stream content, string mimeType, string fileName)
        {
            var stgService = new StorageService();
            var result = await stgService.UploadFileAsync(content, mimeType, fileName, null);
            return result;

        } 

        public async Task<ContentModel> SaveContent(int id, Stream content, string mimeType, ContentType contentType)
        {
            var fileName = id.ToString() + "_" + contentType.ToString() + "_" + Guid.NewGuid().ToString();

            // TODO: sent to DB ..
            if(contentType == ContentType.preview)
            {
                var sqlBuilder = new SQLBuilderS();
                var sql = sqlBuilder.Init()
                        .From("content")
                        .Select("objId", "fileName")
                        .Where("postId", SQLBuilderOperatorsEnum.EQUAL, id)
                        .Where("type", SQLBuilderOperatorsEnum.EQUAL, "'" + contentType + "'")
                        .Build();

                var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

                if(dyna != null)
                {
                    string delSql = $"DELETE FROM content WHERE  postId ='{id}' AND type = '{contentType}'";
                    await this._conn.ExecuteAsync(delSql);
                    await this._storageService.DeleteObject(dyna.fileName as string);
                }
            }

            var tags = new Dictionary<string, string>()
            {
                { "postId", id.ToString() },
                { "contentType", contentType.ToString() }
            };

            var stgService = new StorageService();
            var result = await stgService.UploadFileAsync(content, mimeType, fileName, tags);

            string _sql = $"INSERT INTO content (postId, objId, type) VALUES ('{id}', '{fileName}', '{contentType}') Returning RowId";
            await this._conn.ExecuteAsync(_sql);

            return result;
        }



        public async Task ChangePublishState(long id)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("post")
                        .Select("isPublished")
                        .Select("tags")
                        .Select("tagsCodeSnippets")
                        .Select("typeId")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                        .From("postFTS")
                        .Select("ROWID", "postfts_rowid")
                        .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                        .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

            if (dyna == null) throw new Exception("Not found");

            long isPublished = dyna.isPublished;

            bool newPublicationState = !isPublished.ToBool();

            string _sql = $"UPDATE post SET  isPublished = {newPublicationState.ToInt()} WHERE ROWID = {id}";
            await this._conn.ExecuteAsync(_sql);

            if (dyna.postfts_rowid == null) return;

            var isTypeFound = dyna.typeId is long && dyna.typeId != 0;

            if (!newPublicationState)
            {
                //string sqlDel = $"DELETE FROM postFTS WHERE ROWID = '{id}'";
                //but why? is this redundant?

                string sqlDelStep1 = $"INSERT OR REPLACE INTO postFTS (ROWID, tags, tagsCodeSnippets) VALUES ('{id}', NULL , NULL)";
                await this._conn.ExecuteAsync(sqlDelStep1);

                //string sqlDelStep2 = $"INSERT INTO postFTS(postFTS, rowid, tags, tagsCodeSnippets) VALUES('delete', {id},  NULL , NULL)";
                //await this._conn.ExecuteAsync(sqlDelStep2);
            }
            else if (isTypeFound && dyna.tags != null && !string.IsNullOrEmpty(dyna.tags as string))
            {

                string tsql = string.Empty;

                if (dyna.typeId == 5)
                {
                    tsql = $"INSERT OR REPLACE INTO postFTS (ROWID, tags, tagsCodeSnippets) VALUES ('{id}', NULL ,'{dyna.tagsCodeSnippets}')";
                }
                else
                {
                    tsql = $"INSERT OR REPLACE INTO postFTS (ROWID, tags, tagsCodeSnippets) VALUES ('{id}', '{dyna.tags}', NULL)";

                }


                await this._conn.ExecuteAsync(tsql);

            }
        }
        public async Task<PostModel> Save(PostModel model)
        {
            if (model.Id.HasValue && model.Id.Value != 0)
            {
                await Update(model);
                return model;
            }

            return await Create(model);
        }

        private async Task Update(PostModel model)
        {
            if(model.Type == null || model.Type.Id == 0)
            {
                throw new Exception("type is required");
            }

            //if (model.Tags == null || model.Tags.Count == 0)
            //{
            //    throw new Exception("tags are required");
            //}

            var sqlBuilder = new SQLBuilderS();
            var sqlQ = sqlBuilder.Init()
                        .From("post")
                        .Select("isPublished")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, model.Id.Value)
                        .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sqlQ));

            if (dyna == null) throw new Exception("Not found");

            long isPublished = dyna.isPublished;
            var b64title = !string.IsNullOrEmpty(model.Title) ? model.Title.ToBase64() : string.Empty.ToBase64();
            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            var b64description = !string.IsNullOrEmpty(model.Description) ? model.Description.ToBase64() : string.Empty.ToBase64();
            var typeId = model.Type != null ? model.Type.Id.ToString() : "NULL";
            var tags = model.Tags != null && model.Tags.Any() ? string.Join(" ", model.Tags) : null;

            string sql = string.Empty;

            if (model.Type.Id == 5)
            {
                    //_sql = $"UPDATE postFTS SET tagsCodeSnippets = '{contentAsStr}' WHERE rowid = '{model.Id}' ";
                sql = $"UPDATE post SET title = '{b64title}', content = '{b64Content}', description = '{b64description}', typeId = {typeId}, tagsCodeSnippets = '{tags}' WHERE ROWID = {model.Id}";
            }
            else
            {
                sql = $"UPDATE post SET title = '{b64title}', content = '{b64Content}', description = '{b64description}', typeId = {typeId}, tags = '{tags}' WHERE ROWID = {model.Id}";
            }

            await this._conn.ExecuteAsync(sql, model);
            
            if (tags != null && isPublished.ToBool())
            {
                var contentAsStr = string.Join(" ", model.Tags);

                string _sql = string.Empty;

                if (model.Type.Id == 5)
                {
                    _sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{model.Id}', NULL ,'{contentAsStr}') Returning RowId";
                }
                else
                {
                    _sql = $"INSERT OR REPLACE INTO postFTS (rowid, tags, tagsCodeSnippets) VALUES ('{model.Id}', '{contentAsStr}', NULL) Returning RowId";

                }


                await this._conn.ExecuteAsync(_sql, model);
            }


        }

        private async Task<PostModel> Create(PostModel model)
        {
            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            var b64description = !string.IsNullOrEmpty(model.Description) ? model.Description.ToBase64() : string.Empty.ToBase64();
            var b64title = !string.IsNullOrEmpty(model.Title) ? model.Title : string.Empty.ToBase64();
            var tags = model.Tags != null && model.Tags.Any() ? string.Join(" ", model.Tags) : "NULL";

            var typeId = model.Type != null ? model.Type.Id.ToString() : "NULL";

            string sql = $"INSERT INTO post (title, content, description, typeId, isPublished, createdAt, tags) VALUES ('{b64title}', '{b64Content}', '{b64description}', {typeId}, {false.ToInt()} ,{DateTime.Now.Ticks}, '{tags}') Returning RowId";
            var id = await this._conn.ExecuteScalarAsync<int>(sql, model);

            model.Id = id;
            return model;
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

        private async Task<PaginationModel> Count(int count, int offset, int? postTypeId, string[]? tags, bool? published)
        {
            
            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("post", "pst")
                        .Select("1", "totalElements", SQLBuilderFunctions.COUNT);

            if (published.HasValue)
            {
                q.From("post")
                  .Where("isPublished", SQLBuilderOperatorsEnum.EQUAL, published.Value.ToInt());
            }

            if (postTypeId.HasValue)
            {
                q.From("post")
                  .Where("typeId", SQLBuilderOperatorsEnum.EQUAL, postTypeId.Value);
            }

            if (tags != null && tags.Length > 0)
            {
                var tagsStr = string.Join(" ", tags);

                q.From("postFTS")
                    .Where("postFTS", SQLBuilderOperatorsEnum.EQUAL, "'" + tagsStr + "'")
                    .Join("post", "postFTS", "ROWID", "rowid");
            }

            var sql = q.Build();
            var totalElements = await this._conn.QuerySingleAsync<int>(sql);
            return new PaginationModel(count, offset, totalElements);
        }

        public async Task<PostResult> List(int count, int offset, int? postTypeId, string[]? tags, bool? published)
        {
            var pag = new PaginationModel(count, offset);

            if (count != 0) 
            {
                pag = await Count(count, offset, postTypeId, tags, published);

                if (pag.TotalCount == 0) return new PostResult([], pag);
            }

            var result = new List<PostModel>();

            var contentPreviewSQ = new SQLBuilderS()
                                    .Init()
                                    .From("content", "c")
                                    .Select("ROWID")
                                    .Where("type", SQLBuilderOperatorsEnum.EQUAL, "'" + ContentType.preview + "'")
                                    .Where("postid", SQLBuilderOperatorsEnum.EQUAL, "pst.ROWID");

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("post", "pst")
                        .Select("ROWID", "id")
                        .Select("title")
                        .Select("description")
                        .Select("createdAt", "createdAtTicks")
                        .Select("isPublished")
                        .From("content")
                        .Select("objId", "contentId")
                        .Join("post", "content", "ROWID", "ROWID", contentPreviewSQ, SQLBuilderJoinTypeEnum.LEFT)
                        .From("postType")
                        .Select("ROWID", "typeId")
                        .Select("name", "typeName")
                        .Join("post", "postType", "typeId" ,"ROWID", SQLBuilderJoinTypeEnum.LEFT)
                        .Limit(count)
                        .Offset(offset);

            // TODO:  CHANGE FOR SOMETHING SMARTER
            // MAYBE TELL THE FE WHAT IT IS SPECTING
            //if(postTypeId.HasValue && ( postTypeId.Value == 5 || postTypeId.Value == 4))
            //{
            //    q.From("post").Select("content");
            //}

            if (published.HasValue)
            {
                q.From("post")
                  .Where("isPublished", SQLBuilderOperatorsEnum.EQUAL, published.Value.ToInt());
            }

            if (postTypeId.HasValue)
            {
                q.From("post")
                  .Where("typeId", SQLBuilderOperatorsEnum.EQUAL, postTypeId.Value);
            }

            if (tags != null && tags.Length > 0)
            {
                var tagsStr = string.Join(" ", tags);

                q.From("postFTS")
                    .Select("tags")
                    .Select("tagsCodeSnippets")
                    .Where("postFTS", SQLBuilderOperatorsEnum.EQUAL, "'"+ tagsStr + "'")
                    .Join("post", "postFTS", "ROWID", "rowid");
            }
            else
            {
                q.From("postFTS")
               .Select("tags")
               .Select("tagsCodeSnippets")
               .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT);
            }

            var sql = q.Build();

            var dyna = (await this._conn.QueryAsync(sql));

            if (dyna == null) return new PostResult([], pag);

            foreach ( var dynb in dyna)
            {
                var titleOriginal = (dynb.title as string).FromBase64();
                var descriptionOriginal = dynb.description is string ? (dynb.description as string).FromBase64() : null;
                long isPublished = dynb.isPublished;
                var createdAt = new DateTime(dynb.createdAtTicks);
                //var content = DynamicExtensions.HasProperty(dynb, "content") && dynb.content != null ? (dynb.content as string).FromBase64() : string.Empty;
                var canLoadType = dynb.typeId is long;
                

                var model = new PostModel()
                {
                    Title = titleOriginal,
                    Description = descriptionOriginal,
                    Id = dynb.id,
                    IsPublished = isPublished.ToBool(),
                    Tags = [],
                    CreatedAt = createdAt,
                    //Content = content,
                    Type = canLoadType ? new PostTypeModel() { Id = dynb.typeId, Name = dynb.typeName } : null,
                };


                model.Contents = [];

                if (!string.IsNullOrEmpty(dynb.contentId as string))
                {
                    model.Contents.Add(await _storageService.GenerateDownloadUrl(dynb.contentId));
                }

                if (canLoadType)
                {
                    if (dynb.typeId == 5)
                    {
                        model.Tags = dynb.tagsCodeSnippets is string && !string.IsNullOrEmpty(dynb.tagsCodeSnippets) ? dynb.tagsCodeSnippets.Split(' ') : [];
                    }
                    else
                    {
                        model.Tags = dynb.tags is string && !string.IsNullOrEmpty(dynb.tags) ? dynb.tags.Split(' ') : [];
                    }
                }


                result.Add(model);
               
            }
            return new PostResult(result, pag);
        }


        public async Task<PostModel> Get(int id)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("post")
                        .Select("ROWID", "id")
                        .Select("title")
                        .Select("description")
                        .Select("createdAt", "createdAtTicks")
                        .Select("content")
                        .Select("isPublished")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                        .From("postType", "type") 
                        .Select("ROWID", "typeId") 
                        .Select("name")
                        .Join("post", "postType", "typeId", "ROWID", SQLBuilderJoinTypeEnum.LEFT)
                        .From("postFTS")
                        .Select("tags")
                        .Select("tagsCodeSnippets")
                        .From("content")
                        .Select("objId", "contentId")
                        .Select("type", "imgTtype")
                        .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                        .Join("post", "content", "ROWID", "postid", SQLBuilderJoinTypeEnum.LEFT)
                        .Build();

            var dyna = (await this._conn.QueryAsync(sql));

            if (dyna == null) throw new Exception("Not found");

            var firstE = dyna.FirstOrDefault();

            var titleOriginal = (firstE.title as string).FromBase64();
            var contentOriginal = (firstE.Content as string).FromBase64();
            var descriptionOriginal = firstE.description is string ? (firstE.description as string).FromBase64() : string.Empty.ToBase64() ;
            long isPublished = firstE.IsPublished;
            var canLoadType = firstE.typeId is long;

            var data = new PostModel
            {
                Id = firstE.Id,
                Content = contentOriginal,
                CreatedAt = new DateTime(firstE.createdAtTicks),
                Description = descriptionOriginal,
                Title = titleOriginal,
                IsPublished =  isPublished.ToBool(),
                Type = canLoadType ? new PostTypeModel()
                {
                    Id = firstE.typeId,
                    Name = firstE.name,
                } : null,
                Tags = []   
                
            };

            if (canLoadType)
            {
                if(firstE.typeId == 5)
                {
                    data.Tags = firstE.tagsCodeSnippets is string && !string.IsNullOrEmpty(firstE.tagsCodeSnippets) ? firstE.tagsCodeSnippets.Split(' ') : [];
                }
                else
                {
                    data.Tags = firstE.tags is string && !string.IsNullOrEmpty(firstE.tags) ? firstE.tags.Split(' ') : [];
                }
            }

            //retrieve urls from storage ....
            var validContents = dyna.Where(c => c.contentId != null && c.contentId is string);
            var ctnts = await _storageService.GenerateDownloadUrls(validContents.Select(c => c.contentId as string).ToArray());

            foreach (var item in ctnts)
            {
                var imgTypeStr = validContents.First(c => c.contentId == item.Name).imgTtype as string;

                Enum.TryParse(imgTypeStr, out ContentType imgType);

                item.Type = imgType;
            }

            data.Contents = ctnts;

            return data;
        }


    }
}
