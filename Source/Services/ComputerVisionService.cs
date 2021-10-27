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
        private ComputerVisionClient _client;
        public ComputerVisionService(IConfiguration config)
        {
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(config.GetSection("ComputerVision").GetValue<string>("Key")))
            { Endpoint = config.GetSection("ComputerVision").GetValue<string>("Endpoint") };
        }

        public async Task<IEnumerable<ReadResult>> FindTextOnImageAsync(Stream imageStream)
        {
            var streamHeaders = await _client.ReadInStreamAsync(imageStream);
            string operationLocation = streamHeaders.OperationLocation;

            Thread.Sleep(2000);

            // Retrieve the URI where the extracted text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);
            ReadOperationResult results;
            do
            {
                results = await _client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                    results.Status == OperationStatusCodes.NotStarted));

            return results.AnalyzeResult.ReadResults;
        }
    }
}
