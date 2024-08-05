using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Implementations
{
    /// <summary>
    /// Repository for subcontractor declaration data.
    /// </summary>
    public class SubcontractorDeclarationRepository : ISubcontractorDeclarationRepository
    {
        private readonly IRepository<FullSubcontractorDeclaration> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationRepository"/> class.
        /// </summary>
        /// <param name="repository">Subcontractor declaration repository.</param>
        public SubcontractorDeclarationRepository(IRepository<FullSubcontractorDeclaration> repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<FullSubcontractorDeclaration> GetFullSubcontractorDeclarationById(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }
}
