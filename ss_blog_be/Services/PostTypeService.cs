using ErrorOr;
using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using ss_blog_be.Services.Data;
using ss_blog_be.Storage;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ss_blog_be.Services
{
    public class PostTypeService
    {
        TagDataService tagService;
        PostDataService postDataService;
        PostTypeDataService postTypeService;
        public PostTypeService([Required] SqliteConnection conn)
        {
            tagService = new TagDataService(conn);
            postTypeService = new PostTypeDataService(conn);
        }

        public async Task<ErrorOr<PostTypeModel>> Save(PostTypeModel model)
        {
            if (model == null) return Error.Conflict("Post Type can not be null");
            if (string.IsNullOrEmpty(model.Name)) return Error.Validation("Name can not be empty");

            var postTypeFromDb = await postTypeService.Get(null, model.Name);
            if(postTypeFromDb != null) return Error.Validation("Name is already in use");

            return await postTypeService.Save(model);
        }

        public async Task<ErrorOr<PostTypeModel>> Update(PostTypeModel model)
        {
            if (model == null) return Error.Conflict("Post Type can not be null");
            if (string.IsNullOrEmpty(model.Name)) return Error.Validation("Name can not be empty");

            var postTypeFromDb = await postTypeService.Get(null, model.Name);
            if (postTypeFromDb != null && postTypeFromDb.Id != model.Id) return Error.Validation("Name is already in use");
            if (postTypeFromDb != null && postTypeFromDb.Id == model.Id) return model;

            await postTypeService.Update(model);

            return model;
        }

        public async Task<ErrorOr<PostTypeModel>> Get(int id)
        {
            if(id == default || id < 0) return Error.Conflict("id is not valid");

            var postTypeFromDb = await postTypeService.Get(id, null);
            if (postTypeFromDb == null) return Error.NotFound();

            return postTypeFromDb;
        }

        public async Task<ErrorOr.Deleted> Delete(int id)
        {

            throw new NotImplementedException();
        }
    }
}
