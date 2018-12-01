﻿using Microsoft.Extensions.Configuration;
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

        //delete return bool
        public async Task<bool> DeleteCvAsync(string cvId) 
        {
            _logger.LogInformation($"Starting deleting CV with ID: {cvId}.");

            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);

            if (await blob.ExistsAsync())
                _logger.LogInformation($"Before operation: CV file with ID:{cvId} exist.");
            else
                _logger.LogWarning($"Before operation: CV file with ID:{cvId} not exist.");

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
                throw new InvalidActionException($"Something went wrong while deleting CV file from server.");
            }
            finally
            {
                _logger.LogInformation("Completed operation: Deleting CV from blob.");
            }

            if (await blob.ExistsAsync())
            {
                _logger.LogError($"After operation: CV file with ID:{cvId} still exist.");
                throw new InvalidActionException($"Something went wrong while deleting CV file from server.");
            }
            else
                _logger.LogInformation($"After operation: CV file with ID:{cvId} not exist.");

            return true;
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
            try
            {
                await blob.UploadFromStreamAsync(CvStream);
                _logger.LogInformation("Uploading CV completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogInformation("Uploading CV failed");
                return null;
            }
            finally
            {
                _logger.LogInformation("Completed operation: Uploading CV");
            }

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
                _logger.LogInformation("Completed operation: Copy to archieve");
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
