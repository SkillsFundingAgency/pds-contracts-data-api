using Pds.Contracts.Data.Repository.DataModels;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Interfaces
{
    /// <summary>
    /// Subcontractor Declaration Service.
    /// </summary>
    public interface ISubcontractorDeclarationService
    {
        /// <summary>
        /// Gets the full subcontractor declaration by ID asynchronously.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="Task{FullSubcontractorDeclaration}"/> corresponding to the input parameter(s).</returns>
        Task<FullSubcontractorDeclaration> GetFullSubcontractorDeclarationById(int id);
    }
}
