using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FinanceChargesListener.Boundary.Response;

namespace FinanceChargesListener.Gateway.Services.Interfaces
{
    public interface IAwsS3FileService
    {
        Task<string> UploadFile(IFormFile formFile, string fileName);
        Task<Stream> GetFile(string bucketName, string key);
        Task<bool> DeleteFile(string bucketName, string key);
        Task<bool> UpdateFileTag(string bucketName, string key, string tagValue);
        Task<List<FileProcessingLogResponse>> GetProcessedFiles();
        Task<FileLocationResponse> UploadPrintRoomFile(IFormFile formFile, string fileName);
    }
}
