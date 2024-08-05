using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <summary>
    /// Subcontractor Declaration Service.
    /// </summary>
    public class SubcontractorDeclarationService : ISubcontractorDeclarationService
    {
        private readonly ISubcontractorDeclarationRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationService" /> class.
        /// </summary>
        /// <param name="repository">Subcontractor declaration repository.</param>
        public SubcontractorDeclarationService(
            ISubcontractorDeclarationRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc/>
        public async Task<FullSubcontractorDeclaration> GetFullSubcontractorDeclarationById(int id)
        {
            return await _repository.GetFullSubcontractorDeclarationById(id);
        }
    }
}
