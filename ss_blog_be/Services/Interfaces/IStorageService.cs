namespace ss_blog_be.Services.Interfaces
{
    public interface IStorageService
    {
        Task<string> GenerateUploadUrl(string objectName);
        Task<string> GenerateDownloadUrl(string objectName);
        Task DeleteObject(string objectName);
        Task MoveFilesFromStagingToMain(ICollection<string> files);
    }
}
