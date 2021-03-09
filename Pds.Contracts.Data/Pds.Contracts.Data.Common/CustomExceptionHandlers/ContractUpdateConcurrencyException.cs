using Pds.Contracts.Data.Common.Enums;
using System;


namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// This class handle Contract Update Concurrency Exception.
    /// </summary>
    public class ContractUpdateConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractUpdateConcurrencyException"/> class.
        /// </summary>
        /// <param name="contractNumber">Contract number.</param>
        /// <param name="contractVersion">Contract version number.</param>
        /// <param name="contractId">Contract internal identifier.</param>
        /// <param name="status">Contract status.</param>
        public ContractUpdateConcurrencyException(string contractNumber, int contractVersion, int contractId, ContractStatus status)
           : base($"Contract may have been modified or deleted since Contract were loaded - Contract Id {contractId}, Contract Number: {contractNumber}, ContractVersion: {contractVersion}.")
        {
            ContractNumber = contractNumber;
            ContractVersion = contractVersion;
            ContractId = contractId;
            Status = status;
        }

        /// <summary>
        /// Gets ContractNumber.
        /// </summary>
        public string ContractNumber { get; }

        /// <summary>
        /// Gets VersionNumber.
        /// </summary>
        public int ContractVersion { get; }

        /// <summary>
        /// Gets ContractId.
        /// </summary>
        public int ContractId { get; }

        /// <summary>
        /// Gets or sets status of contract.
        /// </summary>
        public ContractStatus Status { get; set; }
    }
}
