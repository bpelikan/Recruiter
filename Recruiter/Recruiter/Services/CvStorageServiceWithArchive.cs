using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class CvStorageServiceWithArchive : ICvStorageService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger _logger;

        CloudBlobClient blobClient;
        string baseBlobUri;
        string blobAccountName;
        string blobKeyValue;

        CloudBlobClient blobClientArchive;
        string baseBlobUriArchive;
        string blobAccountNameArchive;
        string blobKeyValueArchive;


        public CvStorageServiceWithArchive(IConfiguration configuration, ILogger<CvStorageServiceWithArchive> logger)
        {
            this.configuration = configuration;
            _logger = logger;

            baseBlobUri = configuration["baseBlobUri"];
            blobAccountName = configuration["blobAccountName"];
            blobKeyValue = configuration["blobKeyValue"];
            var credential = new StorageCredentials(blobAccountName, blobKeyValue);
            blobClient = new CloudBlobClient(new Uri(baseBlobUri), credential);

            baseBlobUriArchive = configuration["baseBlobUriArchive"];
            blobAccountNameArchive = configuration["blobAccountNameArchive"];
            blobKeyValueArchive = configuration["blobKeyValueArchive"];
            var credentialArchive = new StorageCredentials(blobAccountNameArchive, blobKeyValueArchive);
            blobClientArchive = new CloudBlobClient(new Uri(baseBlobUriArchive), credentialArchive);

        }

        public async Task<bool> DeleteCvAsync(string cvId)
        {
            _logger.LogInformation($"Starting deleting CV with ID: {cvId}");

            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);

            var result = await blob.DeleteIfExistsAsync();

            return result;
        }

        public async Task<string> SaveCvAsync(Stream CvStream, string userId, string fileName)
        {
            _logger.LogInformation($"Starting uploading CV \nFilename: {fileName} \nUser: {userId}");
            var cvId = userId + "." + Guid.NewGuid().ToString() + "." + Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
            _logger.LogInformation($"Generated ID for CV: {cvId}");

            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);
            blob.Properties.ContentType = "application/pdf";

            _logger.LogInformation("Starting upload");
            await blob.UploadFromStreamAsync(CvStream);
            _logger.LogInformation($"Completed upload CV with ID: {cvId} ");

            var containerArchive = blobClientArchive.GetContainerReference("cvstoragearchieve");
            var blobArchive = containerArchive.GetBlockBlobReference(cvId);
            blobArchive.Properties.ContentType = "application/pdf";

            _logger.LogInformation("Starting copy to archieve");
            try
            {
                var url = new Uri(UriFor(cvId));
                _logger.LogInformation($"Blob url is {url}");
                await blobArchive.StartCopyAsync(url);
                _logger.LogInformation("Copy to archieve completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogInformation("Copy to archieve failed");
            }
            finally
            {
                _logger.LogInformation("Completed Copy to archieve operation");
            }

            return cvId;
        }

        public string UriFor(string cvId)
        {
            _logger.LogInformation($"Generating Url for CV with ID: {cvId}");
            var sasPolicy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-1),
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(1)
            };
            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);
            var sas = blob.GetSharedAccessSignature(sasPolicy);
            return $"{baseBlobUri}cvstorage/{cvId}{sas}";
        }
    }
}
