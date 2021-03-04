using System;

namespace Pds.Contracts.Data.Common.CustomExceptionHandlers
{
    /// <summary>
    /// Represents an error when the contract version is not valid.
    /// </summary>
    public class ContractWithHigherVersionAlreadyExistsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractWithHigherVersionAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="contractVersion">The contract version.</param>
        public ContractWithHigherVersionAlreadyExistsException(string contractNumber, int contractVersion)
            : base($"A contract with ContractNumber [{contractNumber}] and with a higher ContractVersion [{contractVersion}] already exists. We don't accept contracts with a lower version than one that already exists.")
        {
        }
    }
}
