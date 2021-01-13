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
    }
}