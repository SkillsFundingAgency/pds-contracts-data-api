﻿using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Contract service.
    /// </summary>
    public interface IContractService
    {
        /// <summary>
        /// Creates a new record in there database with the requested details.
        /// </summary>
        /// <param name="request">The details of the request.</param>
        /// <returns>The created contract.</returns>
        Task CreateAsync(CreateContractRequest request);

        /// <summary>
        /// Gets a contract by Id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The hello string.</returns>
        Task<Models.Contract> GetAsync(int id);

        /// <summary>
        /// Gets the by contract number and version asynchronous.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="version">The version.</param>
        /// <returns>A contract <see cref="Models.Contract"/>.</returns>
        Task<Models.Contract> GetByContractNumberAndVersionAsync(string contractNumber, int version);

        /// <summary>
        /// Gets the by contract number, version and ukprn asynchronous.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="version">The version.</param>
        /// <param name="ukprn">The ukprn.</param>
        /// <returns>A contract <see cref="Models.Contract"/>.</returns>
        Task<Models.Contract> GetContractAsync(string contractNumber, int version, int ukprn);

        /// <summary>
        /// Gets the contract by contract number.
        /// </summary>
        /// <param name="contractNumber">The contract identifier.</param>
        /// <returns>A list of contracts for a given contract number.</returns>
        Task<IList<Models.Contract>> GetByContractNumberAsync(string contractNumber);

        /// <summary>
        /// Get contract reminders.
        /// </summary>
        /// <param name="reminderInterval">Interval in days.</param>
        /// <param name="pageNumber">The page number to return.</param>
        /// <param name="pageSize">The number of records in the page.</param>
        /// <param name="sort">Sort parameters to apply.</param>
        /// <param name="order">The order in which to .</param>
        /// <param name="templatedQueryString">The templated query string.</param>
        /// <returns>Returns a list of contract reminder response.</returns>
        Task<ContractReminderResponse<IEnumerable<ContractReminderItem>>> GetContractRemindersAsync(int reminderInterval, int pageNumber, int pageSize, ContractSortOptions sort, SortDirection order, string templatedQueryString);

        /// <summary>
        /// Update the LastEmailReminderSent and LastUpdatedAt for a provided id, contract number
        /// and contract version.
        /// </summary>
        /// <param name="request">
        /// An UpdateLastEmailReminderSentRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>Returns a contract service model.</returns>
        Task<Contract> UpdateLastEmailReminderSentAndLastUpdatedAtAsync(UpdateLastEmailReminderSentRequest request);

        /// <summary>
        /// Update the contract status to Approved service. and contract version.
        /// </summary>
        /// <param name="request">
        /// An UpdateConfirmApprovalRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>Returns a Updated Contract Status Response model.</returns>
        Task<UpdatedContractStatusResponse> ConfirmApprovalAsync(UpdateConfirmApprovalRequest request);


        /// <summary>
        /// Update contract if it has a current status of PublishedToProvider to WithdrawByAgency or WithdrawByProvider.
        /// </summary>
        /// <param name="request">
        /// An UpdateContractWithdrawalRequest model containing id, contract number, contract version and withdrawal status.
        /// </param>
        /// <returns>Returns a Updated Contract Status Response model.</returns>
        Task<UpdatedContractStatusResponse> WithdrawalAsync(UpdateContractWithdrawalRequest request);

        /// <summary>
        /// Manual update the contract status to Approved.
        /// </summary>
        /// <param name="request">
        /// An ContractRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>Returns a Updated Contract Status Response model.</returns>
        Task<UpdatedContractStatusResponse> ApproveManuallyAsync(ContractRequest request);

        /// <summary>
        /// Prepend signed page to the document after Approved.
        /// </summary>
        /// <param name="contractId">Contract Id. </param>
        /// <returns>Representing the asynchronous operation.</returns>
        Task PrependSignedPageToDocumentAndSaveAsync(int contractId);
    }
}