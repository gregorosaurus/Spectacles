using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Spectacle.Services
{
    public class ComputerVisionService
    {
        private ComputerVisionClient _computerVisionClient;
        private AzureDataService _azureDataService;
        private string _modelVersion;

        public ComputerVisionService(IConfiguration config, AzureDataService azureDataService)
        {
            _computerVisionClient = new ComputerVisionClient(new ApiKeyServiceClientCredentials(config.GetSection("ComputerVision").GetValue<string>("Key")))
            { Endpoint = config.GetSection("ComputerVision").GetValue<string>("Endpoint") };

            _modelVersion = config.GetSection("ComputerVision").GetValue<string>("ModelVersion") ?? "latest";
            _azureDataService = azureDataService;
        }

        public async Task<IEnumerable<ReadResult>> FindTextOnImageAsync(string imageName, Stream imageStream)
        {
            Uri blobSasUri = await _azureDataService.UploadImageToBlobStorage(imageName, imageStream);

            var streamHeaders = await _computerVisionClient.ReadAsync(blobSasUri.AbsoluteUri, modelVersion: _modelVersion);
            string operationLocation = streamHeaders.OperationLocation;

            Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);
            ReadOperationResult results;
            do
            {
                results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                    results.Status == OperationStatusCodes.NotStarted));

            return results.AnalyzeResult.ReadResults;
        }
    }
}
