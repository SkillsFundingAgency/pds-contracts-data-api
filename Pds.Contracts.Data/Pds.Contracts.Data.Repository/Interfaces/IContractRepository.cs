using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.DataModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Interfaces
{
    /// <summary>
    /// Repository for managing contract workflow.
    /// </summary>
    public interface IContractRepository
    {
        /// <summary>
        /// An example create method.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>Async task.</returns>
        Task CreateAsync(DataModels.Contract contract);

        /// <summary>
        /// Gets the contract by ID asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="Task{Contract}"/> corresponding to the input parameter(s).</returns>
        Task<Contract> GetAsync(int id);

        /// <summary>
        /// Gets the by contract by contract number asynchronously.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <returns>A <see cref="Task{Contract}"/> corresponding to the input parameter(s).</returns>
        Task<IEnumerable<Contract>> GetByContractNumberAsync(string contractNumber);

        /// <summary>
        /// Gets the contract by contract number and version asynchronously.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="version">The version.</param>
        /// <returns>A <see cref="Task{Contract}"/> corresponding to the input parameter(s).</returns>
        Task<Contract> GetByContractNumberAndVersionAsync(string contractNumber, int version);

        /// <summary>
        /// Get Contract Reminders Async.
        /// </summary>
        /// <param name="currentDateTimeMinusNumberOfDays">Current date time minus number of days.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="sort">Sort option.</param>
        /// <param name="order">Sort order.</param>
        /// <returns>IList Contract.</returns>
        Task<IPagedList<Contract>> GetContractRemindersAsync(DateTime currentDateTimeMinusNumberOfDays, int pageNumber, int pageSize, ContractSortOptions sort, SortDirection order);

        /// <summary>
        /// Updates the last email reminder sent date and last updated at date.
        /// </summary>
        /// <param name="contractId">Contract Id.</param>
        /// <returns>Returns a contract model entity using the provided Id.</returns>
        Task<Contract> UpdateLastEmailReminderSentAndLastUpdatedAtAsync(int contractId);

        /// <summary>
        /// Update the contract status to given new status.
        /// </summary>
        /// <param name="contractId">Contract Id.</param>
        /// <param name="requiredContractStatus">required contract status before update.</param>
        /// <param name="newContractStatus">the new contract status.</param>
        /// <returns>
        /// Returns a UpdatedContractStatusResponse model which will have the new and old contract status.
        /// </returns>
        Task<UpdatedContractStatusResponse> UpdateContractStatusAsync(int contractId, ContractStatus requiredContractStatus, ContractStatus newContractStatus);

        /// <summary>
        /// Gets the Contract with Contract content egar loaded by contract Id asynchronously.
        /// </summary>
        /// <param name="id">The  Contract Id.</param>
        /// <returns>An instance of <see cref="Contract"/> with <see cref="ContractContent"/> pre-populated.</returns>
        Task<Contract> GetContractWithContractContentAsync(int id);

        /// <summary>
        /// Gets the Contract with Contract datas egar loaded by contract Id asynchronously.
        /// </summary>
        /// <param name="id">The  Contract Id.</param>
        /// <returns>An instance of <see cref="Contract"/> with <see cref="ContractData"/> pre-populated.</returns>
        Task<Contract> GetContractWithContractDataAsync(int id);

        /// <summary>
        /// Update contract.
        /// </summary>
        /// <param name="contract">Contract to be updated.</param>
        /// <returns>
        /// Async task completion.
        /// </returns>
        public Task UpdateContractAsync(Contract contract);
    }
}