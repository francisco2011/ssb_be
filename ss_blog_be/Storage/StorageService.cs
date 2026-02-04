using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ss_blog_be.Models;
using ss_blog_be.Models.Storage;
using System.Security.AccessControl;

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

        public async Task<StorageObjectModel> Upload(Stream file, string mimeType, string fileName, string bucket, string path)
        {
            fileName = fileName.Replace(@"/", string.Empty);

            var newFileName = (!string.IsNullOrEmpty(path)? path + "/" : string.Empty) + fileName;

            var downloadUrl = await UploadFileAsyncTo(file, mimeType, newFileName, bucket);

            return new StorageObjectModel()
            {
                Id = newFileName,
                Name = newFileName,
                Type = StorageObjectType.File,
                Url = downloadUrl,
            };
        }

        public async Task<StorageObjectModel[]> Traverse(string bucket, string[] folders)
        {
            if (string.IsNullOrEmpty(bucket))
            {
                return await GetAllBucketsAsync();
            }

            var allFoldersAsPrefix = folders != null && folders.Length > 0  ? string.Join("/", folders) : string.Empty;
            allFoldersAsPrefix = string.IsNullOrEmpty(allFoldersAsPrefix) ? allFoldersAsPrefix : allFoldersAsPrefix + "/";
            return await GetContentPerBucketAsync(bucket, allFoldersAsPrefix, "/");
        }

        public async Task<StorageObjectModel[]> GetAllBucketsAsync()
        {
            // The AmazonS3Client automatically picks up credentials
            // from the environment or configuration.
            
                try
                {
                    ListBucketsResponse response = await client.ListBucketsAsync();

                    return response.Buckets.Select(c => new StorageObjectModel() { Name = c.BucketName, Id = c.BucketArn, Type= StorageObjectType.Bucket }).ToArray();
                }
                catch (AmazonS3Exception ex)
                {
                    Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' when listing buckets");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error encountered on server. Message:'{ex.Message}' when listing buckets");
                    throw;
                }
            
        }

        public async Task<StorageObjectModel[]> GetContentPerBucketAsync(string bucketName, string prefix, string delimiter)
        {
            // The AmazonS3Client automatically picks up credentials
            // from the environment or configuration.

            try
            {
                var response = await client.ListObjectsV2Async(new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    Delimiter = delimiter,
                    Prefix = prefix
                });

                var directories = new List<StorageObjectModel>();

                if(response.CommonPrefixes != null)
                {
                    response.CommonPrefixes.ForEach(c =>
                    {
                        var splitArr = c.Split(delimiter);
                        var name = splitArr[splitArr.Length-2]; // dont care about last cuz it will be an empty string ..... 
                        directories.Add(new StorageObjectModel()
                        {
                            Id = name,
                            Name = name,
                            Type = StorageObjectType.Directory,
                        });
                    });
                }

                var objects = new List<StorageObjectModel>();


                if(response.S3Objects != null)
                {

                    response.S3Objects.Where(c => c.Key != prefix).ToList()
                                        .ForEach(async c => {

                                            objects.Add(new StorageObjectModel()
                                            {
                                                Id = c.Key,
                                                Name = c.Key,
                                                Type = (c.Key.EndsWith(delimiter) ? StorageObjectType.Directory : StorageObjectType.File),
                                                Url = await GenerateDownloadUrl(c.Key),
                                                Size = c.Size,
                                                UpdatedOn = c.LastModified
                                                
                                            });
                                        });

                } 
                    

                return directories.Concat(objects).ToArray();
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' when listing buckets");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error encountered on server. Message:'{ex.Message}' when listing buckets");
                throw;
            }

        }





        private async Task<string> UploadFileAsyncTo(Stream file, string mimeType, string fileName, string bucket, IDictionary<string, string> tags = null)
        {

            try
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucket,
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

                return await GenerateDownloadUrl(fileName, bucket);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> UploadFileAsync(Stream file, string mimeType, string fileName, IDictionary<string, string> tags = null)
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

        public async Task<string> GenerateDownloadUrlMainStorage(string objectName)
        {
            return await GenerateDownloadUrl(objectName, Settings.MainBucket);
        }

        public async Task<string> GenerateDownloadUrl(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return null;

            return await GenerateDownloadUrl(objectName, Settings.MainBucket);

        }

        private async Task<string> GenerateDownloadUrl(string objectName, string bucket)
        {

            if (!string.IsNullOrEmpty(Settings.PublicUrl))
            {
                return Settings.PublicUrl + "/" + objectName;
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
            return urlString;

        }

        public async Task DeleteObjectsMatch(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            
            try
            {

                var response = await client.ListObjectsV2Async(new ListObjectsV2Request()
                {
                    BucketName = Settings.MainBucket,
                    Delimiter = "/",
                    Prefix = path
                });

                if (response?.S3Objects == null) return;

                var deleteObjectRequest = new DeleteObjectsRequest
                {
                    BucketName = Settings.MainBucket,
                    Objects = response.S3Objects.Select(x => new KeyVersion() { Key = x.Key }).ToList(),

                };

                Console.WriteLine("Deleting an object");
                await client.DeleteObjectsAsync(deleteObjectRequest);
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

        public async Task DeleteObject(string objectName)
        {

            try
            {

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = Settings.MainBucket,
                    Key = objectName,

                };

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
