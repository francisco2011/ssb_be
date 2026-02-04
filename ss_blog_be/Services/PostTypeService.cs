using ErrorOr;
using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using ss_blog_be.Services.Data;
using ss_blog_be.Storage;
using System.ComponentModel.DataAnnotations;

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

            var postTypeFromDb = postTypeService.Get(null, model.Name);
            if(postTypeFromDb != null) return Error.Validation("Name is already in use");

            return await postTypeService.Save(model);
        }

        public async Task<ErrorOr.Deleted> Delete(int id)
        {

            throw new NotImplementedException();
        }
    }
}
