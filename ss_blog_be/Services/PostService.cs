using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using ss_blog_be.Storage;
using System.ComponentModel.DataAnnotations;

namespace ss_blog_be.Services
{
    public class PostService
    {
        TagService tagservice;
        PostDataService postDataService; 
        public PostService([Required] SqliteConnection conn, StorageService storageService)
        {
            tagservice = new TagService(conn);
            postDataService = new PostDataService(conn, storageService);
        }

        public async Task ChangePublishStatus(int id)
        {
           var result = await postDataService.ChangePublishState(id);
            if (result.IsPublished)
            {
                await tagservice.Restore(id);
            }
            else 
            {
                await tagservice.Delete(id);
            }
            
        }

        public async Task Delete(long id)
        {
            await postDataService.Delete(id);
            await tagservice.Rebuild();
        }

        public async Task<long> Clone(int id)
        {
            var original = await postDataService.Get(id);
            original.Id = null;
            var model = await postDataService.Save(original);
            //will have a value since its a save operation LOL
            return model.Id.Value;
        }


    }
}
