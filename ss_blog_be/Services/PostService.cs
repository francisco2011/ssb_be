using Microsoft.Data.Sqlite;
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

        public async Task ChangePublishStatus(long id)
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


    }
}
