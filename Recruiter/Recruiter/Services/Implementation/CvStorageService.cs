using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Recruiter.CustomExceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services.Implementation
{
    public class CvStorageService : ICvStorageService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<CvStorageService> _stringLocalizer;

        CloudBlobClient blobClient;
        string baseBlobUri;
        string blobAccountName;
        string blobKeyValue;

        public CvStorageService(IConfiguration configuration, ILogger<CvStorageService> logger, IStringLocalizer<CvStorageService> stringLocalizer)
        {
            this.configuration = configuration;
            _logger = logger;
            _stringLocalizer = stringLocalizer;

            baseBlobUri = configuration["baseBlobUri"];
            blobAccountName = configuration["blobAccountName"];
            blobKeyValue = configuration["blobKeyValue"];
            var credential = new StorageCredentials(blobAccountName, blobKeyValue);
            blobClient = new CloudBlobClient(new Uri(baseBlobUri), credential);
        }

        public async Task<bool> DeleteCvAsync(string cvId)
        {
            _logger.LogInformation($"Starting deleting CV with ID: {cvId}.");

            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);

            if (await blob.ExistsAsync())
                _logger.LogInformation($"Before delete operation: CV file with ID:{cvId} exist.");
            else
                _logger.LogWarning($"Before delete operation: CV file with ID:{cvId} not exist.");

            _logger.LogInformation("Starting deleting CV from blob.");
            try
            {
                await blob.DeleteIfExistsAsync();
                _logger.LogInformation("Deleting CV from blob completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError($"Something went wrong while deleting cv with FILENAME:{cvId} in Blob.");
                _logger.LogInformation("Deleting CV from blob failed.");
                throw new BlobOperationException(_stringLocalizer["Something went wrong while deleting CV file from server."]);
            }
            finally
            {
                _logger.LogInformation("Completed operation: Deleting CV from blob.");
            }

            if (await blob.ExistsAsync())
            {
                _logger.LogError($"After delete operation: CV file with ID:{cvId} still exist on server.");
                throw new BlobOperationException(_stringLocalizer["Something went wrong while deleting CV file from server - file still exist on server."]);
            }
            else
                _logger.LogInformation($"After delete operation: CV file with ID:{cvId} not exist.");

            return true;
        }

        public async Task<string> SaveCvAsync(Stream CvStream, string fileName, string userId)
        {
            _logger.LogInformation($"Starting uploading CV \nFilename: {fileName} \nUser: {userId}");
            var cvId = userId + "." + Guid.NewGuid().ToString() + "." + Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
            _logger.LogInformation($"Generated ID for CV: {cvId}");

            var container = blobClient.GetContainerReference("cvstorage");
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference(cvId);
            blob.Properties.ContentType = "application/pdf";

            if (await blob.ExistsAsync())
            {
                _logger.LogError($"Before upload operation: CV file with ID:{cvId} already exist on Blob. (UserID: {userId})");
                throw new BlobOperationException(_stringLocalizer["Something went wrong while upload CV file to server."]);
            }
            else
            {
                _logger.LogInformation($"Before upload operation: CV file with ID:{cvId} not exist. (UserID: {userId})");
            }

            _logger.LogInformation("Starting upload");

            try
            {
                await blob.UploadFromStreamAsync(CvStream);
                _logger.LogInformation("Uploading CV completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError($"Something went wrong while uploading CV with FILENAME:{cvId} to Blob. (UserID: {userId})");
                _logger.LogInformation("Uploading CV failed.");
                throw new BlobOperationException(_stringLocalizer["Something went wrong while upload CV file to server."]);
            }
            finally
            {
                _logger.LogInformation("Completed operation: Uploading CV. (UserID: {userId})");
            }

            if (await blob.ExistsAsync())
            {
                _logger.LogInformation($"After upload operation: CV file with ID:{cvId} exist on Blob. (UserID: {userId})");
            }
            else
            {
                _logger.LogError($"After upload operation: CV file with ID:{cvId} not exist on Blob. (UserID: {userId})");
                throw new BlobOperationException(_stringLocalizer["Something went wrong while upload CV file to server."]);
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
