using AutoMapper;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService" /> class.
        /// </summary>
        /// <param name="repository">Contracts repository.</param>
        /// <param name="mapper">Automapper instance.</param>
        public ContractService(IContractRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<Models.Contract> GetAsync(int id)
        {
            var contract = await _repository.GetAsync(id).ConfigureAwait(false);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public async Task<IList<Models.Contract>> GetByContractNumberAsync(string contractNumber)
        {
            var contracts = await _repository.GetByContractNumberAsync(contractNumber);
            return _mapper.Map<IList<Models.Contract>>(contracts);
        }

        /// <inheritdoc/>
        public async Task<Models.Contract> GetByContractNumberAndVersionAsync(string contractNumber, int version)
        {
            var contract = await _repository.GetByContractNumberAndVersionAsync(contractNumber, version).ConfigureAwait(false); //GetByPredicate(c => c.ContractNumber == contractNumber && c.ContractVersion == version);
            return _mapper.Map<Models.Contract>(contract);
        }
    }
}