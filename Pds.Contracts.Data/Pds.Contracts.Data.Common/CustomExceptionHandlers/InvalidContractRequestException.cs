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
        /// <param name="contractId">Contract internal identifier.</param>
        public InvalidContractRequestException(string contractNumber, int versionNumber, int contractId)
            : base($"Invalid contract request. The contract number: {contractNumber} or contract version: {versionNumber} does not correspond to contract Id: {contractId}.")
        {
            VersionNumber = versionNumber;
            ContractId = contractId;
            ContractNumber = contractNumber;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidContractRequestException"/> class.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="versionNumber">Contract version number.</param>
        /// <param name="contractId">Contract internal identifier.</param>
        /// <param name="withdrawalType">Contract status.</param>
        public InvalidContractRequestException(string contractNumber, int versionNumber, int contractId, ContractStatus withdrawalType)
            : base($"Invalid contract status request. The contract withdrawal type request: {withdrawalType.ToString("G")} The contract number: {contractNumber}, contract version: {versionNumber}, contract Id: {contractId}.")
        {
            VersionNumber = versionNumber;
            ContractId = contractId;
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

        /// <summary>
        /// Gets ContractId.
        /// </summary>
        public int? ContractId { get; }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
