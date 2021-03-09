using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Repository.Extensions;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
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

        private readonly ILoggerAdapter<ContractRepository> _logger;

        private readonly IUnitOfWork _work;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractRepository"/> class.
        /// </summary>
        /// <param name="repository">Contract repository.</param>
        /// <param name="work">The single unit of work.</param>
        /// <param name="logger">The logger.</param>
        public ContractRepository(
            IRepository<Contract> repository,
            IUnitOfWork work,
            ILoggerAdapter<ContractRepository> logger)
        {
            _repository = repository;
            _work = work;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(Contract contract)
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

        /// <inheritdoc/>
        public async Task<Contract> UpdateLastEmailReminderSentAndLastUpdatedAtAsync(int contractId)
        {
            var updatedDate = DateTime.UtcNow;

            var contract = await _repository.GetByIdAsync(contractId);
            if (contract != null)
            {
                contract.LastEmailReminderSent = updatedDate;
                contract.LastUpdatedAt = updatedDate;
                await _work.CommitAsync();
                _logger.LogInformation($"[UpdateLastEmailReminderSentAndLastUpdatedAtAsync] - Updated successfully the last email reminder sent - Contract Id: {contractId}, Contract Number: {contract.ContractNumber}");
            }
            else
            {
                _logger.LogError($"[UpdateLastEmailReminderSentAndLastUpdatedAtAsync] Contract not found - Contract Id {contractId}.");
            }

            return contract;
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> UpdateContractStatusAsync(int contractId, ContractStatus requiredContractStatus, ContractStatus newContractStatus)
        {
            UpdatedContractStatusResponse updatedContractStatusResponse = null;
            var updatedDate = DateTime.UtcNow;
            var contract = await _repository.GetByIdAsync(contractId);
            if (contract != null)
            {
                updatedContractStatusResponse = new UpdatedContractStatusResponse() { Id = contract.Id, ContractNumber = contract.ContractNumber, ContractVersion = contract.ContractVersion, Ukprn = contract.Ukprn, Status = (ContractStatus)contract.Status };

                if (contract.Status == (int)requiredContractStatus)
                {
                    contract.Status = (int)newContractStatus;
                    contract.LastUpdatedAt = updatedDate;
                    await _work.CommitAsync();
                    updatedContractStatusResponse.NewStatus = (ContractStatus)contract.Status;
                    _logger.LogInformation($"[{nameof(UpdateContractStatusAsync)}] - Updated successfully the contract status to {(ContractStatus)contract.Status} - Contract Id: {contractId}, Contract Number: {contract.ContractNumber}");
                }
                else
                {
                    _logger.LogError($"[[{nameof(UpdateContractStatusAsync)}] Contract status is not {requiredContractStatus} - Contract Id {contractId} , the current status is {(ContractStatus)contract.Status}.");
                    throw new ContractStatusException($"Contract status is not {requiredContractStatus} - Contract Id {contractId} , the current status is {(ContractStatus)contract.Status}.");
                }
            }
            else
            {
                _logger.LogError($"[[{nameof(UpdateContractStatusAsync)}] Contract not found - Contract Id {contractId}.");
                throw new ContractNotFoundException($"Contract not found - Contract Id {contractId}.");
            }

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task<Contract> GetContractWithContractContentAsync(int id)
        {
            return await _repository.GetByPredicateWithIncludeAsync(c => c.Id == id, c => c.ContractContent);
        }

        /// <inheritdoc/>
        public async Task<Contract> GetContractWithContractDataAsync(int id)
        {
            return await _repository.GetByPredicateWithIncludeAsync(c => c.Id == id, c => c.ContractData);
        }

        /// <inheritdoc/>
        public async Task UpdateContractAsync(Contract contract)
        {
            contract.LastUpdatedAt = DateTime.UtcNow;
            if (_work.IsTracked(contract))
            {
                try
                {
                await _work.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new ContractUpdateConcurrencyException(contract.ContractNumber, contract.ContractVersion, contract.Id, (ContractStatus)contract.Status);
                }
            }
            else
            {
                await _repository.PatchAsync(contract.Id, contract);
            }
        }
    }
}