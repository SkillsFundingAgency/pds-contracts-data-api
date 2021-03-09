using System;
using System.Runtime.Serialization;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Represents error raised when a blob filename has no content.
    /// </summary>
    [Serializable]
    public class BlobException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobException"/> class.
        /// </summary>
        /// <param name="message">The exception message raised.</param>
        public BlobException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobException"/> class.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="contractVersion">The contract version.</param>
        /// <param name="contractId">The contract id.</param>
        /// <param name="fileName">The blob storage file name.</param>
        /// <param name="message">The raised exception message.</param>
        public BlobException(string contractNumber, int contractVersion, int contractId, string fileName, string message)
            : base($"Exception raised for blob {fileName} for contract with ContractId: {contractId}, ContractNumber: {contractNumber}, ContractVersion: {contractVersion} and ContractVersion: {contractVersion}.  Failed with {message}.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobException"/> class.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="contractVersion">The contract version.</param>
        /// <param name="contractId">The contract id.</param>
        /// <param name="fileName">The blob storage file name.</param>
        /// <param name="innerException">The raised exception message.</param>
        public BlobException(string contractNumber, int contractVersion, int contractId, string fileName, Exception innerException)
            : base($"Exception raised for blob {fileName} for contract with ContractId: {contractId}, ContractNumber: {contractNumber}, ContractVersion: {contractVersion} and ContractVersion: {contractVersion}.", innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobException"/> class.
        /// </summary>
        /// <param name="info">Serialisation info.</param>
        /// <param name="context">Serialisation context.</param>
        protected BlobException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}