using ErrorOr;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using ss_blog_be.Services.Data;
using ss_blog_be.Services.Interfaces;
using ss_blog_be.Storage;
using System.ComponentModel.DataAnnotations;

namespace ss_blog_be.Services
{
    public class PostService
    {
        TagDataService tagService;
        PostDataService postDataService; 
        PostTypeDataService postTypeService;
        StorageService storageService;
        public PostService([Required] SqliteConnection conn, StorageService _storageService)
        {
            tagService = new TagDataService(conn);
            postDataService = new PostDataService(conn, _storageService);
            postTypeService = new PostTypeDataService(conn);
            storageService = _storageService;
        }

        public async Task<ErrorOr<PostModel>> Get(int id)
        {
            var post = await postDataService.Get(id, true);

            if(post == null) return Error.NotFound();
            
            return post;
        }

        public async Task ChangePublishStatus(int id)
        {

            var post = await postDataService.Get(id, false);

            if (post.Type == null || post.Type.Id == default) throw new Exception("Can not change the status of a post without type!");

            var result = await postDataService.ChangePublishState(id, post);

            
            if (result.IsPublished)
            {
                 await tagService.Restore(id, post.Type.Id);
            }
            else
            {
                 await tagService.Delete(id, post.Type.Id);
            }
            
        }

        public async Task Delete(int id)
        {
            var post = await postDataService.Get(id, false);

            await tagService.Delete(id, post.Type.Id, true);

            await postDataService.Delete(id);

            await storageService.DeleteObjectsMatch($"{id}/");

        }

        public async Task<long> Clone(int id)
        {
            var original = await postDataService.Get(id, false);
            original.Id = null;
            var model = await postDataService.Save(original);
            //will have a value since its a save operation LOL
            return model.Id.Value;
        }

    }
}
