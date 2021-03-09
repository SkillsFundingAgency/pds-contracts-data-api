using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Data.Services.ConfigurationOptions
{
    /// <summary>
    /// Configuration option for the blob container client.
    /// </summary>
    public class BlobContainerClientOptions
    {
        /// <summary>
        /// Gets or sets Blob connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets Blob container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets Blob options retry count.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets Blob options delay.
        /// </summary>
        public TimeSpan Delay { get; set; }
    }
}
