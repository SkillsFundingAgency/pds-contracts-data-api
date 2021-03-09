using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Represents error raised when a blob filename has no content.
    /// </summary>
    public class BlobNoContentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobNoContentException"/> class.
        /// </summary>
        /// <param name="message">Custom exception message.</param>
        public BlobNoContentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobNoContentException"/> class.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="contractVersion">The contract version.</param>
        /// <param name="contractId">The contract id.</param>
        /// <param name="fileName">The blob storage file name.</param>
        public BlobNoContentException(string contractNumber, int contractVersion, int contractId, string fileName)
            : base($"No content found for blob {fileName} for contract with ContractId: {contractId}, ContractNumber: {contractNumber} and ContractVersion: {contractVersion}.")
        {
        }
    }
}