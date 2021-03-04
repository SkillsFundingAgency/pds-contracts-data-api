using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Contract validation service.
    /// </summary>
    public interface IContractValidationService
    {
        /// <summary>
        /// Validates the status change.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="newStatus">The new status.</param>
        /// <param name="isManualApproval">true if it is a manual approval, default it is false.</param>
        void ValidateStatusChange(Contract contract, ContractStatus newStatus, bool isManualApproval = false);

        /// <summary>
        /// Validates the contract from database for the request.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="request">The contract request used to find contract.</param>
        void Validate(Contract contract, Models.ContractRequest request);

        /// <summary>
        /// Validates the contract from database for the request.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <param name="request">The contract request used to find contract.</param>
        /// <param name="validatePredicate">Validation expression.</param>
        void Validate(Contract contract, Models.ContractRequest request, Expression<Func<Contract, bool>> validatePredicate);

        /// <summary>
        /// Validates the contract request against the list of contracts in the database.
        /// Checks if there is a contract with the same or higher version.
        /// </summary>
        /// <param name="request">Request to create the contract.</param>
        /// <param name="existingContracts"><see cref="IEnumerable{Contract}"/> of contracts that have the same contract number.</param>
        /// <exception cref="Pds.Contracts.Data.Common.CustomExceptionHandlers.DuplicateContractException">Raised if a contract with a given contract number and version already exists.</exception>
        /// <exception cref="Pds.Contracts.Data.Common.CustomExceptionHandlers.ContractWithHigherVersionAlreadyExistsException">Raised if a contract with a given contract number but a higher version already exists.</exception>
        void ValidateForNewContract(Models.CreateContractRequest request, IEnumerable<Contract> existingContracts);
    }
}