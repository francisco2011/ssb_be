using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ss_blog_be.Models;

namespace ss_blog_be.Storage
{
    public class StorageService
    {

        //private const string bucketName = "blg-cntnt-sb";
        private const string bucketNameStaging = "blg-cntnt-sb-staging";
        //private static readonly RegionEndpoint bucketRegion = RegionEndpoint.CACentral1;
        private IAmazonS3 client = null;
        private StorageSettings Settings = null;

        public StorageService(StorageSettings settings)
        {
            Settings = settings;


            var config = new AmazonS3Config
            {
                
                 ServiceURL = settings.StorageUrl, // Your R2 endpoint,
                 
            //    RegionEndpoint = RegionEndpoint.USEast1,  // RegionEndpoint.USEast1, // Use a default region like USEast1
            //     AuthenticationRegion = "auto", // R2 uses 'auto' region
            //DisablePayloadSigning = true, // Must be true for R2 uploads
            // DisableDefaultChecksumValidation = true // May be required
            };

            try
            {

                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(settings.StorageAccessKeyId, settings.StorageAccessKey);
                client = new AmazonS3Client(awsCredentials, config);
            }
            catch(Exception e)
            {
                var a = 1;
            }

        }

        public async Task MoveFilesFromStagingToMain(ICollection<string> files)
        {
            if (files.Count == 0) return;

            try
            {

                var allCopyRequests = files.Select(c =>
                {
                    var copyFileRequest = new CopyObjectRequest
                    {
                        SourceBucket = bucketNameStaging,
                        SourceKey = c,
                        DestinationBucket = Settings.MainBucket,
                        DestinationKey = c,//Put archive folder path here
                    };

                    return client.CopyObjectAsync(copyFileRequest);

                });

                await Task.WhenAll(allCopyRequests);

                var allDeleteReq = files.Select(c =>
                {
                    return client.DeleteObjectAsync(bucketNameStaging, c);
                });

                await Task.WhenAll(allDeleteReq);

            }
            catch (Exception ex)
            {
                //TODO: Next do not ignore errors ....
                Console.Error.WriteLine(ex.ToString());
                //throw;
            }
        }



        public async Task<ContentModel> UploadFileAsync(Stream file, string mimeType, string fileName, IDictionary<string, string> tags = null)
        {

            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = Settings.MainBucket,
                    Key = fileName,
                    InputStream = file,
                    DisablePayloadSigning = true, //required by r2 on 2026-01-11
                    DisableDefaultChecksumValidation = true //required by r2 on 2026-01-11
                };

                //not supported by r2 on 2026-01-11
                //if (tags != null && tags.Any())
                //{
                //    uploadRequest.TagSet = tags.Select(c => new Tag() { Key = c.Key, Value = c.Value }).ToList();
                //}

                using (TransferUtility tranUtility =
                new TransferUtility(client))
                {
                    await tranUtility.UploadAsync(uploadRequest);

                }

                return await GenerateDownloadUrl(fileName, Settings.MainBucket);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<ContentModel> GenerateUploadUrl(ContentModel model)
        {

            var id = Guid.NewGuid();
            model.Name = id + "_" + model.Name;

            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucketNameStaging,
                    ContentType = model.MimeType,
                    Key = model.Name,
                    Expires = DateTime.Now.AddMinutes(5),
                    Verb = HttpVerb.PUT
                };
                urlString = await client.GetPreSignedURLAsync(request1);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            model.Url = urlString;
            return model;
        }

        public async Task<ContentModel> GenerateDownloadUrlMainStorage(string objectName)
        {
            return await GenerateDownloadUrl(objectName, Settings.MainBucket);
        }

        public async Task<ICollection<ContentModel>> GenerateDownloadUrls(string[] objectNames)
        {
            if (objectNames.Length == 0) return [];

            var allUrls = objectNames.Select(c => GenerateDownloadUrl(c, Settings.MainBucket));

            return await Task.WhenAll(allUrls);

        }

        public async Task<ContentModel> GenerateDownloadUrl(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return null;

            return await GenerateDownloadUrl(objectName, Settings.MainBucket);

        }

        private async Task<ContentModel> GenerateDownloadUrl(string objectName, string bucket)
        {

            if (!string.IsNullOrEmpty(Settings.PublicUrl))
            {
                return new ContentModel() { Name = objectName, Url = Settings.PublicUrl + "/" + objectName };
            }

            string urlString = "";
            try
            {
                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucket,
                    Key = objectName,
                    Expires = DateTime.Now.AddMinutes(5),
                    Verb = HttpVerb.GET
                };
                urlString = await client.GetPreSignedURLAsync(request1);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            return new ContentModel() { Name = objectName, Url = urlString };

        }

        public async Task DeleteObject(string objectName)
        {

            try
            {

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = Settings.MainBucket,
                    Key = objectName,

                };

                Console.WriteLine("Deleting an object");
                await client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }

    }
}
