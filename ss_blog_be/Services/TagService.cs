using Microsoft.Data.Sqlite;
using ss_blog_be.Models;
using System.ComponentModel.DataAnnotations;

namespace ss_blog_be.Services
{
    public class TagService
    {
        private SqliteConnection _conn { get; }
        TagDataService tagservice;
        PostDataService postDataService;
        PostTypeService postTypeService;

        public TagService([Required] SqliteConnection conn)
        {
            _conn = conn;
            
            tagservice = new TagDataService(conn);
            postDataService = new PostDataService(conn, null);
            postTypeService = new PostTypeService(conn);

        }

        public async Task Update(int postId, string[] tags)
        {
            var post = await postDataService.Get(postId, false);

            await tagservice.UpdateTags(postId, post.Type.Id, tags);
        }

        public async Task<TagOcurrencesModel[]> Get(int postTypeid)
        {
            var postType = await postTypeService.Get(postTypeid);

            if (postType == null) throw new Exception("Post type not registered");

            return await tagservice.GetTags(postTypeid);
        }
    }
}
