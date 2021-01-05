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
        /// <returns>
        /// The hello string.
        /// </returns>
        Task<Models.Contract> GetAsync(int id);

        /// <summary>
        /// Gets the contract by contract number.
        /// </summary>
        /// <param name="contractNumber">The contract identifier.</param>
        /// <returns>
        /// A list of contracts for a given contract number.
        /// </returns>
        public IList<Models.Contract> GetByContractNumber(string contractNumber);
    }
}