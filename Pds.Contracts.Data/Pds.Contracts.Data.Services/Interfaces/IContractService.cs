using Pds.Contracts.Data.Common.Enums;
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
    }
}