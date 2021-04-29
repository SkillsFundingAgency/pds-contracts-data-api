using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Pds.Contracts.Data.Services.ConfigurationOptions;

namespace Pds.Contracts.Data.Services.Helpers
{
    /// <summary>
    /// Blob helper class.
    /// </summary>
    public class BlobHelper
    {
        /// <summary>
        /// Get an instance BlobContainerClient.
        /// </summary>
        /// <param name="configuration">Configuration options.</param>
        /// <returns>Returns an instance of BlobContainerClient.</returns>
        public static BlobContainerClient GetBlobContainerClient(IConfiguration configuration)
        {
            var blobContainerClientOptions = new BlobContainerClientOptions();

            configuration.GetSection("AzureBlobConfiguration").Bind(blobContainerClientOptions);
            var blobClientOptions = new BlobClientOptions();
            blobClientOptions.Retry.MaxRetries = blobContainerClientOptions.RetryCount;
            blobClientOptions.Retry.Mode = Azure.Core.RetryMode.Exponential;
            blobClientOptions.Retry.Delay = blobContainerClientOptions.Delay;

            return new BlobContainerClient(blobContainerClientOptions.ConnectionString, blobContainerClientOptions.ContainerName, blobClientOptions);
        }
    }
}
