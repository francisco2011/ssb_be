using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using ss_blog_be.Services.Data;
using System.ComponentModel.DataAnnotations;

namespace ss_blog_be.Services
{
    public class TagService
    {
        private UOW _uow { get; }
        
        public TagService([Required] SqliteConnection conn)
        {
            _uow = new UOW(conn);
        }

        public async Task Update(int postId, string[] tags)
        {
            await _uow.BeginTransaction();

            try
            {
                var post = await _uow.PostDataService.Get(postId, false);
                await _uow.TagService.UpdateTags(postId, post.Type.Id, tags);

                await _uow.CommitTransaction();
            }
            catch (Exception ex)
            {
               await _uow.RollbackTransaction();
            }
        }

        public async Task<TagOcurrencesModel[]> Get(int postTypeid)
        {
            var postType = await postTypeService.Get(postTypeid, null);

            if (postType == null) throw new Exception("Post type not registered");

            return await tagservice.GetTags(postTypeid);
        }
    }
}
