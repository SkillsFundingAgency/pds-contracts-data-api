using AutoMapper;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractService : IContractService
    {
        private readonly IRepository<Contract> repository;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService" /> class.
        /// </summary>
        /// <param name="repository">Contracts repository.</param>
        /// <param name="mapper">Automapper instance.</param>
        public ContractService(IRepository<Contract> repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<Models.Contract> GetAsync(int id)
        {
            var contract = await repository.GetByIdAsync(id).ConfigureAwait(false);
            return mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public IList<Models.Contract> GetByContractNumber(string contractNumber)
        {
            var contracts = repository.GetMany(c => c.ContractNumber.Equals(contractNumber, System.StringComparison.OrdinalIgnoreCase)).ToList();
            return mapper.Map<IList<Models.Contract>>(contracts.ToList());
        }
    }
}