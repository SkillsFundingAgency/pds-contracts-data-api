using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Represents error raised when creating a contract that already exists.
    /// </summary>
    public class DuplicateContractException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateContractException"/> class.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="contractVersion">The contract version.</param>
        public DuplicateContractException(string contractNumber, int contractVersion)
            : base($"A contract with ContractNumber [{contractNumber}] and ContractVersion [{contractVersion}] already exists.")
        {
        }
    }
}