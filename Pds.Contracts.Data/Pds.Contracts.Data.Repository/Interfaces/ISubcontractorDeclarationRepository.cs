using Pds.Contracts.Data.Repository.DataModels;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Interfaces
{
    /// <summary>
    /// Repository for managing subcontractor declaration workflow.
    /// </summary>
    public interface ISubcontractorDeclarationRepository
    {
        /// <summary>
        /// Gets the full subcontractor declaration by ID asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="Task{FullSubcontractorDeclaration}"/> corresponding to the input parameter(s).</returns>
        Task<FullSubcontractorDeclaration> GetFullSubcontractorDeclarationById(int id);
    }
}
