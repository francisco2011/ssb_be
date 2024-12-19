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

postApi.MapPut("/{id}/changePublishState", async ([FromRoute] long id) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    await dataService.ChangePublishState(id);

    return Results.NoContent();
});

postApi.MapGet("/", async (HttpContext context, [FromQuery] int limit, [FromQuery] int offset, [FromQuery] int? typeId, [FromQuery] string[] tags, [FromQuery] bool? published) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.List(limit, offset, typeId, tags, published);
    return Results.Ok(result);
});

postApi.MapGet("/{id}", async ([FromRoute] int id) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.Get(id);
    return Results.Ok(result);
});

postApi.MapPost("/{id}/tags", async ([FromBody] TagsModel model, [FromRoute] long id) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    //await dataService.AddTags(id, model);
    return Results.NoContent();
});

postApi.MapPost("/{id}/contentType/{contentTypeId}", async ([FromRoute] int id, [FromRoute] ContentType contentTypeId, [FromForm] IFormFile file) =>
{
    var stream = file.OpenReadStream();
    var type = file.ContentType;

    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.SaveContent(id,stream,type, contentTypeId);

    return Results.Ok(result);
}).DisableAntiforgery();

var tagsApi = app.MapGroup("/tags");

tagsApi.MapGet("", async ([FromQuery] int? postTypeId) =>
{
    PostDataService dataService = new PostDataService(new ConnectionBuilder().Connect(), new StorageService());
    var result = await dataService.GetTags(postTypeId);
    return Results.Ok(result);
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


app.Run();

[JsonSerializable(typeof(IEnumerable<PostModel>))]
[JsonSerializable(typeof(IEnumerable<TagModel>))]
[JsonSerializable(typeof(IEnumerable<PostTypeModel>))]
[JsonSerializable(typeof(PostModel))]
[JsonSerializable(typeof(PostTypeModel))]
[JsonSerializable(typeof(TagsModel))]
[JsonSerializable(typeof(TagModel))]
[JsonSerializable(typeof(ContentModel))]
[JsonSerializable(typeof(PaginationModel))]
[JsonSerializable(typeof(PostResult))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}

