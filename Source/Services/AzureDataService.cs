using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spectacle.Services
{
    public class AzureDataService
    {
        private const string SpectaclesUploadContainerName = "uploads";
        private BlobServiceClient _blobServiceClient;
        public AzureDataService(IConfiguration config)
        {
            string connectionString = config.GetConnectionString("StorageAccount");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        /// <summary>
        /// Uploads stream to blob storage and then 
        /// returns a SAS URI to consume. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        public async Task<Uri> UploadImageToBlobStorage(string name, Stream imageStream)
        {
            BlobContainerClient containerClient = await _blobServiceClient.CreateBlobContainerAsync(SpectaclesUploadContainerName);
            await containerClient.CreateIfNotExistsAsync();

            string uploadName = string.IsNullOrEmpty(name) ? Guid.NewGuid().ToString() : name;

            BlobClient blobClient = containerClient.GetBlobClient(uploadName);
            await blobClient.DeleteIfExistsAsync();
            await blobClient.UploadAsync(imageStream);
            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                {"UploadedAt",DateTime.UtcNow.ToString("yyyy-MM-dd\\THH:mm:ss\\Z") }
            });

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.UtcNow.AddMinutes(15));
        }
    }
}
