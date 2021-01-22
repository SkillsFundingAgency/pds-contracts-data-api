using AutoMapper;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractService : IContractService
    {
        private readonly IRepository<Contract> _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService" /> class.
        /// </summary>
        /// <param name="repository">Contracts repository.</param>
        /// <param name="mapper">Automapper instance.</param>
        public ContractService(IRepository<Contract> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<Models.Contract> GetAsync(int id)
        {
            var contract = await _repository.GetByIdAsync(id).ConfigureAwait(false);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public IList<Models.Contract> GetByContractNumber(string contractNumber)
        {
            var contracts = _repository.GetMany(c => c.ContractNumber == contractNumber);
            return _mapper.Map<IList<Models.Contract>>(contracts);
        }

        /// <inheritdoc/>
        public Models.Contract GetByContractNumberAndVersion(string contractNumber, int version)
        {
            var contract = _repository.GetByPredicate(c => c.ContractNumber == contractNumber && c.ContractVersion == version);
            return _mapper.Map<Models.Contract>(contract);
        }
    }
}