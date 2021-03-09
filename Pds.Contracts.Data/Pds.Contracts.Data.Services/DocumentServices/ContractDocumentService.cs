using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataModels = Pds.Contracts.Data.Repository.DataModels;

namespace Pds.Contracts.Data.Services.DocumentServices
{
    /// <inheritdoc/>
    public class ContractDocumentService : IContractDocumentService
    {
        private readonly BlobContainerClient _blobContainerClient = null;
        private readonly ILoggerAdapter<IContractDocumentService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractDocumentService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration options.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="blobContainerClient">The blob container client for blob maintenance.</param>
        public ContractDocumentService(BlobContainerClient blobContainerClient, ILoggerAdapter<IContractDocumentService> logger)
        {
            _logger = logger;
            _logger.LogInformation($"[{nameof(ContractDocumentService)}] instantiating...");
            _blobContainerClient = blobContainerClient;
            _logger.LogInformation($"[{nameof(ContractDocumentService)}] instantiated.");
        }

        /// <inheritdoc/>
        public async Task UpsertOriginalContractXmlAsync(DataModels.Contract contract, ContractRequest request)
        {
            _logger.LogInformation($"[{nameof(UpsertOriginalContractXmlAsync)}] called with contract number: {request.ContractNumber} and contract Id: {request.Id}.");
            if (contract.ContractData is null)
            {
                contract.ContractData = new DataModels.ContractData();
                contract.ContractData.Id = contract.Id;
            }

            contract.ContractData.OriginalContractXml = await GetDocumentContentAsync(request);
        }

        private async Task<string> GetDocumentContentAsync(ContractRequest request)
        {
            _logger.LogInformation($"[{nameof(GetDocumentContentAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} and file name: {request.FileName} ");
            string documentContent = null;
            try
            {
                BlobClient blob = _blobContainerClient.GetBlobClient(request.FileName);
                if (!blob.Exists())
                {
                    throw new BlobException("Blob name does not exist.");
                }

                var response = await blob.DownloadAsync();
                using var reader = new StreamReader(response.Value.Content);
                documentContent = reader.ReadToEnd();
            }
            catch (BlobException ex)
            {
                _logger.LogError($"[{nameof(GetDocumentContentAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} and file name: {request.FileName}.  Failed with {ex.Message}.");
                throw new BlobException(request.ContractNumber, request.ContractVersion, request.Id, request.FileName, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(GetDocumentContentAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} and file name: {request.FileName}.  Failed with {ex.Message}.");
                throw new BlobException(request.ContractNumber, request.ContractVersion, request.Id, request.FileName, ex);
            }

            if (string.IsNullOrWhiteSpace(documentContent))
            {
                _logger.LogError($"[{nameof(GetDocumentContentAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} and file name: {request.FileName}.  Failed because blob filename has no content.");
                throw new BlobNoContentException(request.ContractNumber, request.ContractVersion, request.Id, request.FileName);
            }

            return documentContent;
        }
    }
}
