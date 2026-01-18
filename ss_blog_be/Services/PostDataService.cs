using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;
using Dapper;
using ss_blog_be.Common.SQLBuilder;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Types;
using ss_blog_be.Common.Extensions;
using ss_blog_be.Storage;
using System.Net.WebSockets;
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
            var newUrl = await _storageService.UploadFileAsync(content, mimeType, fileName, null);
            return new ContentModel() { Url = newUrl,  MimeType = mimeType, Name = fileName };

        } 

        public async Task<ContentModel> SaveContent(int id, Stream content, string mimeType, ContentType contentType)
        {
            var fileName = id.ToString() + "_" + contentType.ToString() + "_" + Guid.NewGuid().ToString();

            // There can be only 1 preview and 1 render ....
            if(contentType == ContentType.preview || contentType == ContentType.render
                || contentType == ContentType.descriptionRender || contentType == ContentType.titleRender)
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

            var url = await _storageService.UploadFileAsync(content, mimeType, fileName, tags);

            string _sql = $"INSERT INTO content (postId, objId, type) VALUES ('{id}', '{fileName}', '{contentType}') Returning RowId";
            await this._conn.ExecuteAsync(_sql);

            return new ContentModel() { Name = fileName, Type = contentType, Url = url, PostId = id, MimeType = mimeType};
        }

        public async Task Delete(long id)
        {

            try
            {
                var __sqlBuilder = new SQLBuilderS();
                var __sql = __sqlBuilder.Init()
                            .Delete()
                            .From("postFTS")
                            .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                            .Build();

                await this._conn.ExecuteAsync(__sql);

                var _sqlBuilder = new SQLBuilderS();
                var _sql = _sqlBuilder.Init()
                            .Delete()
                            .From("content")
                            .Where("postid", SQLBuilderOperatorsEnum.EQUAL, id)
                            .Build();

                await this._conn.ExecuteAsync(_sql);

                var sqlBuilder = new SQLBuilderS();
                var sql = sqlBuilder.Init()
                            .Delete()
                            .From("post")
                            .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                            .Build();

                await this._conn.ExecuteAsync(sql);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public async Task<PostModel> ChangePublishState(int id)
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

            if (dyna.postfts_rowid == null) throw new Exception("Can not change the status of a post without type"); ;

            long isPublished = dyna.isPublished;

            bool newPublicationState = !isPublished.ToBool();

            string _sql = $"UPDATE post SET  isPublished = {newPublicationState.ToInt()} WHERE ROWID = {id}";
            await this._conn.ExecuteAsync(_sql);

            return new PostModel()
            {
                Id = id,
                Type = new PostTypeModel() { Id = dyna.typeId },
                IsPublished = newPublicationState
            };

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

            var sqlBuilder = new SQLBuilderS();
            var sqlQ = sqlBuilder.Init()
                        .From("post")
                        .Select("isPublished")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, model.Id.Value)
                        .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sqlQ));

            if (dyna == null) throw new Exception("Not found");

            var b64title = !string.IsNullOrEmpty(model.Title) ? model.Title.ToBase64() : string.Empty.ToBase64();
            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            var b64description = !string.IsNullOrEmpty(model.Description) ? model.Description.ToBase64() : string.Empty.ToBase64();
            var typeId = model.Type != null ? model.Type.Id.ToString() : "NULL";
            var name = !string.IsNullOrEmpty(model.Name)? model.Name : string.Empty;

            string sql = $"UPDATE post SET name = '{name}', title = '{b64title}', content = '{b64Content}', description = '{b64description}', typeId = {typeId} WHERE ROWID = {model.Id}";

            await this._conn.ExecuteAsync(sql, model);
        }

        private async Task<PostModel> Create(PostModel model)
        {
            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            var b64description = !string.IsNullOrEmpty(model.Description) ? model.Description.ToBase64() : string.Empty.ToBase64();
            var b64title = !string.IsNullOrEmpty(model.Title) ? model.Title.ToBase64() : string.Empty.ToBase64();
            var tags = model.Tags != null && model.Tags.Any() ? string.Join(" ", model.Tags) : "";

            var typeId = model.Type != null ? model.Type.Id.ToString() : "NULL";

            string sql = $"INSERT INTO post (title, content, description, typeId, isPublished, createdAt, tags, name) VALUES ('{b64title}', '{b64Content}', '{b64description}', {typeId}, {false.ToInt()} ,{DateTime.Now.Ticks}, '{tags}', '') Returning RowId";
            var id = await this._conn.ExecuteScalarAsync<int>(sql, model);

            model.Id = id;
            return model;
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

        public async Task<PostResult> List(int count, int offset, int? postTypeId, string[]? tags, bool? published, ContentType[] contentsToAdd)
        {
            var pag = new PaginationModel(count, offset);

            if (count != 0) 
            {
                pag = await Count(count, offset, postTypeId, tags, published);

                if (pag.TotalCount == 0) return new PostResult([], pag);
            }

            var result = new List<PostModel>();
            
            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("post", "pst")
                        .Select("ROWID", "id")
                        .Select("name", "name")
                        //.Select("title")
                        //.Select("description")
                        .Select("createdAt", "createdAtTicks")
                        .Select("isPublished")
                        //.From("content")
                        //.Select("objId", "contentId")
                        //.Join("post", "content", "ROWID", "ROWID", contentPreviewSQ, SQLBuilderJoinTypeEnum.LEFT)
                        .From("postType")
                        .Select("ROWID", "typeId")
                        .Select("name", "typeName")
                        .Join("post", "postType", "typeId" ,"ROWID", SQLBuilderJoinTypeEnum.LEFT)
                        .Limit(count)
                        .Offset(offset);

            
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
                //var titleOriginal = string.IsNullOrEmpty(dynb.title)? string.Empty : (dynb.title as string).FromBase64();
                //var descriptionOriginal = dynb.description is string ? (dynb.description as string).FromBase64() : null;
                long isPublished = dynb.isPublished;
                var createdAt = new DateTime(dynb.createdAtTicks);
                //var content = DynamicExtensions.HasProperty(dynb, "content") && dynb.content != null ? (dynb.content as string).FromBase64() : string.Empty;
                var canLoadType = dynb.typeId is long;
                var name = dynb.name is string ? dynb.name as string : string.Empty;
                var id = dynb.id is long ? Convert.ToInt32(dynb.id) : 0;

                var model = new PostModel()
                {
                    Name = name,
                    //Title = titleOriginal,
                    //Description = descriptionOriginal,
                    Id = id,
                    IsPublished = isPublished.ToBool(),
                    Tags = [],
                    CreatedAt = createdAt,
                    //Content = content,
                    Type = canLoadType ? new PostTypeModel() { Id = dynb.typeId, Name = dynb.typeName } : null,
                };

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

            var contents = await getContentFor(result.Where(c => c.Id.HasValue).Select(c => c.Id.Value).ToArray(), contentsToAdd);
            result.ForEach(c =>
            {
                var contentFor = contents.Where(d => d.PostId == c.Id).ToArray();
                c.Contents = contentFor;
            });

            return new PostResult(result, pag);
        }

        private async Task<ContentModel[]> getContentFor(int[] postIds, ContentType[] contentsToAdd)
        {
            var contents = new List<ContentModel>();

            if (contentsToAdd != null && contentsToAdd.Any())
            {
                var sql = new SQLBuilderS()
                        .Init()
                        .From("content", "c")
                        .Select("objId", "name")
                        .Select("type", "type")
                        .Select("postId", "postId")
                        .Where("type", SQLBuilderOperatorsEnum.IN, contentsToAdd.Select(c => c.ToString()).ToArray())
                        .Where("postId", SQLBuilderOperatorsEnum.IN, postIds).Build();

                 var contentsFromDb = (await this._conn.QueryAsync(sql)).ToArray();

                 foreach (var item in contentsFromDb)
                 {
                    var name = item.name is string ? item.name as string : string.Empty;
                    var url = string.IsNullOrEmpty(name) ? string .Empty : await _storageService.GenerateDownloadUrl(name);
                    var postId = Convert.ToInt32(DynamicExtensions.GetPropertyValueAs<long>(item, "postId", 0));

                    ContentType type = default;

                    if(Enum.TryParse(item.type as string, out ContentType _type))
                    {
                        type = _type;
                    }


                    contents.Add(new ContentModel() { Name = name, Url = url, Type = type, PostId = postId });
                 }
            }

            return contents.ToArray();
        } 

        public async Task<PostModel> Get(int id)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("post")
                        .Select("ROWID", "id")
                        .Select("name")
                        .Select("title")
                        .Select("description")
                        .Select("createdAt", "createdAtTicks")
                        .Select("content")
                        .Select("isPublished")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                        .From("postType", "type") 
                        .Select("ROWID", "typeId") 
                        .Select("name", "typeName")
                        .Join("post", "postType", "typeId", "ROWID", SQLBuilderJoinTypeEnum.LEFT)
                        .From("postFTS")
                        .Select("tags")
                        .Select("tagsCodeSnippets")
                        .From("content")
                        .Select("objId", "contentId")
                        .Select("type", "imgType")
                        .Join("post", "postFTS", "ROWID", "rowid", SQLBuilderJoinTypeEnum.LEFT)
                        .Join("post", "content", "ROWID", "postid", SQLBuilderJoinTypeEnum.LEFT)
                        .Build();

            var dyna = (await this._conn.QueryAsync(sql));

            if (dyna == null) throw new Exception("Not found");

            var firstE = dyna.FirstOrDefault();

            if (firstE == null) throw new Exception("Not found");

            PostModel data = PostModel.From(firstE);
            var canLoadType = data.Type.Id != default;

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

            foreach (var c in validContents)
            {
                var newContent = new ContentModel();

                var url = await _storageService.GenerateDownloadUrl(c.contentId as string);
                newContent.Url = url;


                var type = c.imgType;
                
                if(Enum.TryParse(type, out ContentType imgType))
                {
                    newContent.Type = imgType;
                }
                
                data.Contents.Add(newContent);
            }

            return data;
        }


    }
}
