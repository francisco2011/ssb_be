using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ss_blog_be.Models;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace ss_blog_be.Services
{
    public class StorageService
    {

        private const string bucketName = "blg-cntnt-sb";
        private const string bucketNameStaging = "blg-cntnt-sb-staging";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.CACentral1;
        private static IAmazonS3 s3Client;

        public StorageService()
        {
            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("x", "x");
            s3Client = new AmazonS3Client(awsCredentials, bucketRegion);
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
                        DestinationBucket = bucketName,
                        DestinationKey = c,//Put archive folder path here
                    };

                    return s3Client.CopyObjectAsync(copyFileRequest);
                    
                });

                await Task.WhenAll(allCopyRequests);

                var allDeleteReq = files.Select(c =>
                {
                    return s3Client.DeleteObjectAsync(bucketNameStaging, c);
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
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = file
                };

                if(tags != null && tags.Any())
                {
                    uploadRequest.TagSet = tags.Select(c => new Tag() { Key = c.Key, Value = c.Value }).ToList();
                }

                using (TransferUtility tranUtility =
                new TransferUtility(s3Client))
                {
                    await tranUtility.UploadAsync(uploadRequest);

                }

                return await GenerateDownloadUrl(fileName, bucketName);
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
                    urlString = await s3Client.GetPreSignedURLAsync(request1);
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
            return await GenerateDownloadUrl(objectName, bucketName);
        }

        public async Task<ICollection<ContentModel>> GenerateDownloadUrls(string[] objectNames)
        {
            if (objectNames.Length == 0) return [];

            var allUrls = objectNames.Select(c => GenerateDownloadUrl(c, bucketName));   

            return await Task.WhenAll(allUrls);
            
        }

        public async Task<ContentModel> GenerateDownloadUrl(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return null;

            return await GenerateDownloadUrl(objectName, bucketName);

        }


        private async Task<ContentModel> GenerateDownloadUrl(string objectName, string bucket)
        {

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
                urlString = await s3Client.GetPreSignedURLAsync(request1);
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

        public async Task DeleteObjectsWith(IDictionary<string, string> tags)
        {
            if (tags == null || !tags.Any()) return;

            
            var _tags = tags.Select(c => new Tag() { Key = c.Key, Value = c.Value }).ToList();


            //var deleteObjectRequest = new DeleteObjectsRequest
            //{
            //    BucketName = bucketName,
            //    Key = objectName
            //};

        }

        public async Task DeleteObject(string objectName)
        {

            try
            {

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectName,
                    
                };

                Console.WriteLine("Deleting an object");
                await s3Client.DeleteObjectAsync(deleteObjectRequest);
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
