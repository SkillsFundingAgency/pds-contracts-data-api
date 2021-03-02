using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using System;
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
    }
}