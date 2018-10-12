using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.Services
{
    public class CvStorageService : ICvStorageService
    {
        private readonly IConfiguration configuration;
        CloudBlobClient blobClient;
        string baseBlobUri;
        string blobAccountName;
        string blobKeyValue;

        public CvStorageService(IConfiguration configuration)
        {
            this.configuration = configuration;
            baseBlobUri = configuration["baseBlobUri"];
            blobAccountName = configuration["blobAccountName"];
            blobKeyValue = configuration["blobKeyValue"];
            var credential = new StorageCredentials(blobAccountName, blobKeyValue);
            blobClient = new CloudBlobClient(new Uri(baseBlobUri), credential);
        }

        public async Task<bool> DeleteCvAsync(string cvId)
        {
            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);

            var result = await blob.DeleteIfExistsAsync();

            return result;
        }

        public async Task<string> SaveCvAsync(Stream CvStream, string userId, string fileName)
        {
            var cvId = userId + "." + Guid.NewGuid().ToString() + "." + Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
            var container = blobClient.GetContainerReference("cvstorage");
            var blob = container.GetBlockBlobReference(cvId);
            blob.Properties.ContentType = "application/pdf";
            await blob.UploadFromStreamAsync(CvStream);
            return cvId;
        }

        public string UriFor(string cvId)
        {
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
