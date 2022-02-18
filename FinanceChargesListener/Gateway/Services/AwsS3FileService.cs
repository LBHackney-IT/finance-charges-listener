using Amazon.S3;
using Amazon.S3.Model;
using FinanceChargesListener.Gateway.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FinanceChargesListener.Gateway.Services
{
    public class AwsS3FileService : IAwsS3FileService
    {
        private static readonly Amazon.RegionEndpoint _region = Amazon.RegionEndpoint.EUWest2;
        private AmazonS3Client _s3Client;

        public AwsS3FileService()
        {
            //string accessKey = "ASIAVJ447R6IU7YPFD6P";
            //string secretKey = "GAMBS0pnOMEOyzULG+laTkEEkIIB+dhmiJsl1D2t";

            _s3Client = new AmazonS3Client(_region);
        }

        public async Task<string> UploadFile(IFormFile formFile, string fileName)
        {
            // var location = $"uploads/{formFile.FileName}";
            var location = $"uploads/{fileName}";
            var bucketName = Environment.GetEnvironmentVariable("CHARGES_BUCKET_NAME");
            using (var stream = formFile.OpenReadStream())
            {
                var putRequest = new PutObjectRequest
                {
                    Key = location,
                    BucketName = bucketName,
                    InputStream = stream,
                    AutoCloseStream = true,
                    ContentType = formFile.ContentType
                };
                try
                {
                    await _s3Client.PutObjectAsync(putRequest).ConfigureAwait(false);
                    return location;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to upload file to S3", ex.InnerException);
                }
            }
        }

        public async Task<Stream> GetFile(string bucketName, string key)
        {
            try
            {
                var response = await _s3Client.GetObjectAsync(bucketName, key).ConfigureAwait(false);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? response.ResponseStream : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download file from S3", ex.InnerException);
            }
        }

        public async Task<bool> DeleteFile(string bucketName, string key)
        {
            try
            {
                var response = await _s3Client.DeleteObjectAsync(bucketName, key).ConfigureAwait(false);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete file in S3", ex.InnerException);
            }
        }

        public async Task<bool> UpdateFileTag(string bucketName, string key, string tagValue)
        {
            Tagging newTagSet = new Tagging();
            newTagSet.TagSet = new List<Tag>{
                        new Tag { Key = Constants.TagKey, Value = tagValue}
                    };
            PutObjectTaggingRequest putObjTagsRequest = new PutObjectTaggingRequest()
            {
                BucketName = bucketName,
                Key = key,
                Tagging = newTagSet
            };
            try
            {
                var response = await _s3Client.PutObjectTaggingAsync(putObjTagsRequest);
                if (response != null && !string.IsNullOrEmpty(response.VersionId))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update the file tag in S3", ex.InnerException);
            }

        }
    }
}
