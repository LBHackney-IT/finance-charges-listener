using Amazon.S3;
using Amazon.S3.Model;
using FinanceChargesListener.Gateway.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinanceChargesListener.Boundary.Response;

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
            var request = new GetObjectTaggingRequest
            {
                BucketName = bucketName,
                Key = key
            };
            var taggingResponse = await _s3Client.GetObjectTaggingAsync(request).ConfigureAwait(false);

            newTagSet.TagSet = new List<Tag>{
                new Tag { Key = Constants.TagKey, Value = tagValue}
            };

            foreach (var tag in taggingResponse.Tagging)
            {
                if (newTagSet.TagSet.All(t => t.Key != tag.Key))
                {
                    newTagSet.TagSet.Add(new Tag
                    {
                        Key = tag.Key,
                        Value = tag.Value
                    });
                }
            }

            var putObjTagsRequest = new PutObjectTaggingRequest()
            {
                BucketName = bucketName,
                Key = key,
                Tagging = newTagSet
            };

            try
            {
                var response = await _s3Client.PutObjectTaggingAsync(putObjTagsRequest).ConfigureAwait(false);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update the file tag in S3 {ex.Message}", ex.InnerException);
            }

        }

        public async Task<List<FileProcessingLogResponse>> GetProcessedFiles()
        {
            var prefix = "uploads/";
            var bucketName = Environment.GetEnvironmentVariable("CHARGES_BUCKET_NAME");

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var listObjectResponse = await _s3Client.ListObjectsV2Async(request).ConfigureAwait(false);

            var s3ObjectList = listObjectResponse.S3Objects.OrderByDescending(o => o.LastModified).Take(20).ToList();

            var filesList = new List<FileProcessingLogResponse>();

            foreach (var s3Object in s3ObjectList)
            {
                var (year, fileStatus, valuesType) = await GetObjectTags(s3Object.Key, bucketName).ConfigureAwait(false);
                var fileUrl = GeneratePreSignedUrl(s3Object.Key, bucketName);
                filesList.Add(new FileProcessingLogResponse
                {
                    FileName = s3Object.Key,
                    FileStatus = fileStatus,
                    FileUrl = new Uri(fileUrl),
                    DateUploaded = s3Object.LastModified,
                    Year = year,
                    ValuesType = valuesType
                });
            }

            return filesList;
        }

        public async Task<FileLocationResponse> UploadPrintRoomFile(IFormFile formFile, string fileName)
        {
            var location = $"printoutput/{fileName}";
            var bucketName = Environment.GetEnvironmentVariable("CHARGES_BUCKET_NAME");

            using (var stream = formFile.OpenReadStream())
            {
                var tagSet = new List<Tag> { new Tag { Key = "fileType", Value = "PrintRoom" } };
                var putRequest = new PutObjectRequest
                {
                    Key = location,
                    BucketName = bucketName,
                    InputStream = stream,
                    AutoCloseStream = true,
                    ContentType = "text/csv",
                    TagSet = tagSet
                };
                try
                {
                    await _s3Client.PutObjectAsync(putRequest).ConfigureAwait(false);
                    return new FileLocationResponse
                    {
                        RelativePath = location,
                        BucketName = bucketName,
                        StepNumber = 1,
                        WriteIndex = 0,
                        FileUrl = null
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to upload file to S3  {ex.Message}", ex.InnerException);
                }
            }
        }

        private string GeneratePreSignedUrl(string objectKey, string bucketName)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }

        private async Task<(string year, string fileStatus, string valuesType)> GetObjectTags(string objectKey, string bucketName)
        {
            var request = new GetObjectTaggingRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            var taggingResponse = await _s3Client.GetObjectTaggingAsync(request).ConfigureAwait(false);
            var year = taggingResponse.Tagging.Where(t => t.Key == "year").Select(t => t.Value).FirstOrDefault();
            var fileStatus = taggingResponse.Tagging.Where(t => t.Key == "status").Select(t => t.Value).FirstOrDefault();
            var valuesType = taggingResponse.Tagging.Where(t => t.Key == "valuesType").Select(t => t.Value).FirstOrDefault();
            return (year, fileStatus, valuesType);
        }
    }
}
