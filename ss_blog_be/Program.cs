using Dapper;
using Microsoft.AspNetCore.Mvc;
using ss_blog_be.Models;
using ss_blog_be.Services;
using ss_blog_be.Types;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);

var postApi = app.MapGroup("/post");
postApi.MapPost("/", async (PostModel newModel) => 
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.Save(newModel);

    return Results.Created($"/{result.Id}", result);
});

postApi.MapDelete("/{id}", async ([FromRoute] long id) =>
{
    PostService dataService = new PostService(new ConnectionBuilder().Connect(), new StorageService());
    await dataService.Delete(id);

    return Results.NoContent();
});

postApi.MapPut("/{id}/changePublishState", async ([FromRoute] long id) =>
{
    
    PostService service = new PostService(new ConnectionBuilder().Connect(), new StorageService());
    await service.ChangePublishStatus(id);

    return Results.NoContent();
});

postApi.MapGet("/", async (HttpContext context, [FromQuery] int limit, [FromQuery] int offset, [FromQuery] int? typeId, [FromQuery] string[] tags, [FromQuery] bool? published, [FromQuery] bool? loadContent) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.List(limit, offset, typeId, tags, published, loadContent);
    return Results.Ok(result);
});

postApi.MapGet("/{id}", async ([FromRoute] int id) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.Get(id);
    return Results.Ok(result);
});


postApi.MapPost("/{id}/contentType/{contentTypeId}", async ([FromRoute] int id, [FromRoute] ContentType contentTypeId, [FromForm] IFormFile file) =>
{
    var stream = file.OpenReadStream();
    var type = file.ContentType;

    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.SaveContent(id,stream,type, contentTypeId);

    return Results.Ok(result);
}).DisableAntiforgery();

postApi.MapPut("/{id}/content/{fileName}", async ([FromRoute] int id, [FromRoute] string fileName, [FromForm] IFormFile file) =>
{
    var stream = file.OpenReadStream();
    var type = file.ContentType;

    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.UpdateContent(id, stream, type, fileName);

    return Results.Ok(result);
}).DisableAntiforgery();

var tagsApi = app.MapGroup("/tags");

tagsApi.MapGet("", async ([FromQuery] int? postTypeId) =>
{
    TagService dataService = new TagService(new ConnectionBuilder().Connect());
    var result = await dataService.GetTags(postTypeId);
    return Results.Ok(result);
});

tagsApi.MapPut("/{id}", async ([FromRoute] int id, [FromBody] TagUpdateModel model) =>
{
    TagService dataService = new TagService(new ConnectionBuilder().Connect());
    await dataService.UpdateTags(id, model.tags);
    return Results.NoContent();
});

var postTypeApi = app.MapGroup("/postType");

postTypeApi.MapGet("", async () =>
{
    PostTypeService dataService = new PostTypeService(new ConnectionBuilder().Connect());
    var result = await dataService.Get();
    return Results.Ok(result);
});

var contentApi = app.MapGroup("/content");

contentApi.MapGet("/{name}", async ([FromRoute] string name) =>
{
    StorageService service = new StorageService();
    var result = await service.GenerateDownloadUrlMainStorage(name);

    return Results.Ok(result);
});

var sectionApi = app.MapGroup("/section");

sectionApi.MapPost("", async ([FromBody] SectionModel model) =>
{
    SectionService service = new SectionService(new ConnectionBuilder().Connect());
    var result = await service.Save(model);

    return Results.Ok(result);
});

sectionApi.MapPut("/{id}", async ([FromRoute] int id, [FromBody] SectionModel model) =>
{
    SectionService service = new SectionService(new ConnectionBuilder().Connect());
    await service.Update(id, model);

    return Results.NoContent();
});

sectionApi.MapGet("", async ([FromQuery] int limit, [FromQuery] int offset, [FromQuery] string[] tags, [FromQuery]bool? includeContent) =>
{
    SectionService dataService = new SectionService(new ConnectionBuilder().Connect());
    var result = await dataService.List(limit, offset, tags, includeContent);
    return Results.Ok(result);
});

sectionApi.MapGet("/{id}", async ([FromRoute] int id) =>
{
    SectionService dataService = new SectionService(new ConnectionBuilder().Connect());
    var result = await dataService.Get(id);
    return Results.Ok(result);
});

sectionApi.MapDelete("/{id}", async ([FromRoute] int id) =>
{
    SectionService service = new SectionService(new ConnectionBuilder().Connect());
    await service.Delete(id);

    return Results.NoContent();
});


app.Run();

[JsonSerializable(typeof(IEnumerable<PostModel>))]
[JsonSerializable(typeof(IEnumerable<TagModel>))]
[JsonSerializable(typeof(IEnumerable<PostTypeModel>))]
[JsonSerializable(typeof(IEnumerable<SectionModel>))]
[JsonSerializable(typeof(PostModel))]
[JsonSerializable(typeof(PostTypeModel))]
[JsonSerializable(typeof(TagsModel))]
[JsonSerializable(typeof(TagModel))]
[JsonSerializable(typeof(ContentModel))]
[JsonSerializable(typeof(PaginationModel))]
[JsonSerializable(typeof(PostResult))]
[JsonSerializable(typeof(TagUpdateModel))]
[JsonSerializable(typeof(SectionModel))]
[JsonSerializable(typeof(SectionResult))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

