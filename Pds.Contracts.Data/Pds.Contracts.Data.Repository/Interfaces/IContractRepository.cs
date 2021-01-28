using Pds.Contracts.Data.Repository.DataModels;
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
        /// <returns>
        /// Async task.
        /// </returns>
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
    }
}