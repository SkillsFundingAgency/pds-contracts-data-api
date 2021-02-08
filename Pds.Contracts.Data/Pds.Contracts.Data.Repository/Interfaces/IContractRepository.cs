using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Implementations;
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
        Task ExampleCreate(DataModels.Contract contract);

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
    }
}