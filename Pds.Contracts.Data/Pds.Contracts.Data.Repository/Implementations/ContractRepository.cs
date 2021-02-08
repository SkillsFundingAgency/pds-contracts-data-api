using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Extensions;
using Pds.Contracts.Data.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Repository.Implementations
{
    /// <summary>
    /// Repository for contracts data except blob document.
    /// </summary>
    /// <seealso cref="Pds.Contracts.Data.Repository.Interfaces.IContractRepository"/>
    public class ContractRepository : IContractRepository
    {
        private readonly IRepository<Contract> _repository;

        private readonly IUnitOfWork _work;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRepository"/> class.
        /// </summary>
        /// <param name="repository">Contract repository.</param>
        /// <param name="work">The single unit of work.</param>
        public ContractRepository(IRepository<Contract> repository, IUnitOfWork work)
        {
            _repository = repository;
            _work = work;
        }

        /// <inheritdoc/>
        public async Task ExampleCreate(Contract contract)
        {
            await _repository.AddAsync(contract);
            await _work.CommitAsync();
        }

        /// <inheritdoc/>
        public async Task<Contract> GetAsync(int id)
        {
            return await _repository.GetByIdAsync(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<Contract> GetByContractNumberAndVersionAsync(string contractNumber, int version)
        {
            return await _repository.GetByPredicateAsync(c => c.ContractNumber == contractNumber && c.ContractVersion == version);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Contract>> GetByContractNumberAsync(string contractNumber)
        {
            return await _repository.GetMany(c => c.ContractNumber == contractNumber).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IPagedList<Contract>> GetContractRemindersAsync(DateTime currentDateTimeMinusNumberOfDays, int pageNumber, int pageSize, ContractSortOptions sort, SortDirection order)
        {
            //return all contracts that need reminders sent.
            var query = _repository
                .GetMany(q =>
                    ((q.LastEmailReminderSent == null && currentDateTimeMinusNumberOfDays >= q.CreatedAt) ||
                     (q.LastEmailReminderSent != null && currentDateTimeMinusNumberOfDays >= q.LastEmailReminderSent))
                    && q.Status == (int)ContractStatus.PublishedToProvider);

            var sortedContracts = query.OrderByDynamic(sort.ToString(), order);

            return await sortedContracts.ToPagedList(pageNumber, pageSize);
        }
    }
}