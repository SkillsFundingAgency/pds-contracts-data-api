using Pds.Contracts.Data.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Contract bad request exception.
    /// </summary>
    [Serializable]
    public class InvalidContractRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidContractRequestException"/> class.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="versionNumber">Contract version number.</param>
        /// <param name="withdrawalType">Contract status.</param>
        public InvalidContractRequestException(string contractNumber, int versionNumber, ContractStatus? withdrawalType = null)
            : base($"Invalid contract status request. The contract withdrawal type request: {withdrawalType?.ToString()} The contract number: {contractNumber} and contract version: {versionNumber}.")
        {
            VersionNumber = versionNumber;
            ContractNumber = contractNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidContractRequestException"/> class.
        /// </summary>
        /// <param name="info">Serialisation info.</param>
        /// <param name="context">Serialisation context.</param>
        protected InvalidContractRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets ContractNumber.
        /// </summary>
        public string ContractNumber { get; }

        /// <summary>
        /// Gets VersionNumber.
        /// </summary>
        public int? VersionNumber { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
