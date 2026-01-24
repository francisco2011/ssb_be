using Dapper;
using Microsoft.Data.Sqlite;
using ss_blog_be.Common.SQLBuilder.Enums;
using ss_blog_be.Common.SQLBuilder;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;
using ss_blog_be.Common.Extensions;
using System.Reflection;
using Amazon.S3.Model;

namespace ss_blog_be.Services
{
    public class SectionService
    {
        private SqliteConnection _conn { get; }
        
        public SectionService([Required] SqliteConnection conn)
        {
            _conn = conn;

        }

        public async Task Update(int id, SectionModel model)
        {
            var sqlBuilder = new SQLBuilderS();
            var sql = sqlBuilder.Init()
                        .From("section")
                        .Select("ROWID", "id")
                        .Select("tag", "tag")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                        .Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(sql));

            if (dyna == null) throw new Exception("Not found");

            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            var b64ContentHtml = !string.IsNullOrEmpty(model.ContentHtml) ? model.ContentHtml.ToBase64() : string.Empty.ToBase64();
            var tag = string.IsNullOrEmpty(dyna.tag as string) || (dyna.tag as string) == "{{}}" ?  "{{" + model.Name.Replace(" ", "_") + "}}" : dyna.tag;

            string _sql = $"UPDATE section SET content = '{b64Content}', name = '{model.Name}', tag = '{tag}', contentHtml = '{b64ContentHtml}' WHERE ROWID = {model.Id}";
            await this._conn.ExecuteAsync(_sql);

        }

        public async Task<SectionModel> Save(SectionModel model)
        {
            var b64Content = !string.IsNullOrEmpty(model.Content) ? model.Content.ToBase64() : string.Empty.ToBase64();
            
            //INSERT INTO section (name, content, tag, modifiable) VALUES ('title', '', '{{title}}', 0);
            string _sql = $"INSERT INTO section (name, content, contentHtml, tag, modifiable,createdAt) VALUES ('{model.Name}', '{model.Content}', '{""}', '{""}', 1, {DateTime.Now.Ticks}) Returning RowId";
            var id = await this._conn.ExecuteScalarAsync<int>(_sql, model);

            model.Id = id;
            return model;
        }

        private async Task<PaginationModel> Count(int count, int offset)
        {

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("section", "sct")
                        .Select("1", "totalElements", SQLBuilderFunctions.COUNT);

            var sql = q.Build();
            var totalElements = await this._conn.QuerySingleAsync<int>(sql);
            return new PaginationModel(count, offset, totalElements);
        }

        public async Task<SectionResult> List(int count, int offset, string[] tags, bool? includeContent)
        {

            var pag = new PaginationModel(count, offset);

            if (count != 0)
            {
                pag = await Count(count, offset);

                if (pag.TotalCount == 0) return new SectionResult([], pag);
            }

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("section")
                        .Select("ROWID", "id")
                        .Select("name", "name")
                        .Select("tag", "tag")
                        .Select("modifiable", "modifiable");

            if(includeContent.HasValue && includeContent.Value) q.Select("content", "content").Select("contentHtml", "contentHtml");
            
            if (tags != null && tags.Any()) 
                q.Where("tag", SQLBuilderOperatorsEnum.IN, "("+ string.Join(",",tags.Select(c => "'" + c + "'").ToArray()) +")");
            
            q.Limit(count).Offset(offset);

            var sql = q.Build();

            var dyna = (await this._conn.QueryAsync(sql));

            if (dyna == null) return new SectionResult([], pag);

            var result = new List<SectionModel>();

            foreach (var dynb in dyna)
            {
                long modifiable = dynb.modifiable;

                var name = dynb.name;
                var tag = dynb.tag;
                var id = dynb.id;
                var content = DynamicExtensions.HasProperty(dynb, "content") && !string.IsNullOrEmpty(dynb.content as string) ? (dynb.content as string).FromBase64() : string.Empty;
                var contentHtml = DynamicExtensions.HasProperty(dynb, "contentHtml") && !string.IsNullOrEmpty(dynb.contentHtml as string) ? (dynb.contentHtml as string).FromBase64() : string.Empty;

                result.Add(new SectionModel { Id = id, Name = name, Tag = tag, Modifiable = modifiable.ToBool(), Content = content, ContentHtml = contentHtml });
                
            }

            return new SectionResult(result, pag); ;
        }

        public async Task<SectionModel> Get(int id)
        {
            var result = new List<SectionModel>();

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("section")
                        .Select("ROWID", "id")
                        .Select("name", "name")
                        .Select("tag", "tag")
                        .Select("content", "content")
                        .Select("modifiable", "modifiable")
                        .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id).Build();

            var dyna = (await this._conn.QueryFirstOrDefaultAsync(q));

            if (dyna == null) throw new Exception("Not found");


                long modifiable = dyna.modifiable;

                var name = dyna.name;
                var tag = dyna.tag;
                var _id = dyna.id;
                var content = DynamicExtensions.HasProperty(dyna, "content") && !string.IsNullOrEmpty(dyna.content as string) ? (dyna.content as string).FromBase64() : string.Empty;

                return new SectionModel { Id = _id, Name = name, Tag = tag, Modifiable = modifiable.ToBool(), Content = content };
            

        }

        public async Task<List<SectionModel>> Get()
        {
            var result = new List<SectionModel>();

            var sqlBuilder = new SQLBuilderS();
            var q = sqlBuilder.Init()
                        .From("section")
                        .Select("ROWID", "id")
                        .Select("name", "name")
                        .Select("tag", "tag")
                        //.Select("content", "content")
                        .Select("modifiable","modifiable");

            var sql = q.Build();

            var dyna = (await this._conn.QueryAsync(sql));

            if (dyna == null) return result;

            foreach (var dynb in dyna)
            {
                long modifiable = dynb.modifiable;

                var name = dynb.name; 
                var tag = dynb.tag;
                var id = dynb.id;
                //var content = DynamicExtensions.HasProperty(dynb, "content") && !string.IsNullOrEmpty( dynb.content as string) ? (dynb.content as string).FromBase64() : string.Empty;

                result.Add(new SectionModel { Id = id, Name = name, Tag = tag, Modifiable = modifiable.ToBool() });
            }

            return result;
        }

        public async Task Delete(long id)
        {

            try
            {
                var __sqlBuilder = new SQLBuilderS();
                var __sql = __sqlBuilder.Init()
                            .Delete()
                            .From("section")
                            .Where("ROWID", SQLBuilderOperatorsEnum.EQUAL, id)
                            .Build();

                await this._conn.ExecuteAsync(__sql);

                
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
