﻿using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;


namespace Pds.Contracts.Data.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractService : IContractService
    {
        private const string _appName = "Pds.Contracts.Data.Api";

        private readonly IContractRepository _repository;

        private readonly IMapper _mapper;

        private readonly IUriService _uriService;

        private readonly ILoggerAdapter<ContractService> _logger;

        private readonly IAuditService _auditService;

        private readonly ISemaphoreOnEntity<string> _semaphoreOnEntity;

        private readonly IDocumentManagementContractService _documentService;
        private readonly IContractValidationService _contractValidator;
        private readonly IMediator _mediator;

        private readonly IContractDocumentService _contractDocumentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractService" /> class.
        /// </summary>
        /// <param name="repository">Contracts repository.</param>
        /// <param name="mapper">Automapper instance.</param>
        /// <param name="uriService">The uri service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="auditService">The audit service used for auditing.</param>
        /// <param name="semaphoreOnEntity">The semaphore to use for locking.</param>
        /// <param name="documentService">The document management Contract Service.</param>
        /// <param name="contractValidator">The contract validator.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="contractDocumentService">The blob container used to communicate with azure storage.</param>
        public ContractService(
            IContractRepository repository,
            IMapper mapper,
            IUriService uriService,
            ILoggerAdapter<ContractService> logger,
            IAuditService auditService,
            ISemaphoreOnEntity<string> semaphoreOnEntity,
            IDocumentManagementContractService documentService,
            IContractValidationService contractValidator,
            IMediator mediator,
            IContractDocumentService contractDocumentService)
        {
            _repository = repository;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _auditService = auditService;
            _semaphoreOnEntity = semaphoreOnEntity;
            _documentService = documentService;
            _contractValidator = contractValidator;
            _mediator = mediator;
            _contractDocumentService = contractDocumentService;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(CreateContractRequest request)
        {
            await _semaphoreOnEntity.WaitAsync(request.ContractNumber).ConfigureAwait(false);

            try
            {
                _logger.LogInformation($"[{nameof(CreateAsync)}] Creating new contract [{request.ContractNumber}] version [{request.ContractVersion}] for [{request.UKPRN}].");

                var existing = await _repository.GetByContractNumberAsync(request.ContractNumber);
                _contractValidator.ValidateForNewContract(request, existing);

                var newContract = _mapper.Map<Repository.DataModels.Contract>(request);
                newContract.LastUpdatedAt = newContract.CreatedAt = DateTime.UtcNow;

                // For amendment type notification the default status has to be approved
                // For None and Variation, it should be published to provider
                if (request.AmendmentType == ContractAmendmentType.Notfication)
                {
                    newContract.Status = (int)ContractStatus.Approved;
                    newContract.SignedOn = request.SignedOn.Value.Date;
                    newContract.SignedBy = "Feed";
                    newContract.SignedByDisplayName = "Feed";
                }
                else if (request.AmendmentType == ContractAmendmentType.None || request.AmendmentType == ContractAmendmentType.Variation)
                {
                    newContract.Status = (int)ContractStatus.PublishedToProvider;
                }

                await _contractDocumentService.UpsertOriginalContractXmlAsync(newContract, new ContractRequest() { FileName = request.ContractData, ContractNumber = request.ContractNumber, ContractVersion = request.ContractVersion });

                await _repository.CreateAsync(newContract);

                var updatedContractStatusResponse = new UpdatedContractStatusResponse
                {
                    Id = newContract.Id,
                    ContractNumber = newContract.ContractNumber,
                    ContractVersion = newContract.ContractVersion,
                    Ukprn = newContract.Ukprn,
                    NewStatus = (ContractStatus)newContract.Status,
                    Action = ActionType.ContractCreated,
                    AmendmentType = request.AmendmentType
                };

                _logger.LogInformation($"[{nameof(CreateAsync)}] Contract [{newContract.ContractNumber}] version [{newContract.ContractVersion}] has been created for [{newContract.Ukprn}].");

                // Update operations to existing records can be done outside the semaphore
                if (request.AmendmentType == ContractAmendmentType.Variation || request.AmendmentType == ContractAmendmentType.None)
                {
                    var statuses = new int[]
                        {
                            (int)ContractStatus.PublishedToProvider
                        };

                    await ReplaceContractsWithGivenStatuses(existing, statuses);
                }
                else if (request.AmendmentType == ContractAmendmentType.Notfication)
                {
                    var statuses = new int[]
                        {
                            (int)ContractStatus.Approved,
                            (int)ContractStatus.ApprovedWaitingConfirmation,
                            (int)ContractStatus.PublishedToProvider
                        };

                    await ReplaceContractsWithGivenStatuses(existing, statuses);
                }

                await _mediator.Publish(updatedContractStatusResponse);
            }
            finally
            {
                _semaphoreOnEntity.Release(request.ContractNumber);
            }
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
            var contract = await _repository.GetByContractNumberAndVersionAsync(contractNumber, version).ConfigureAwait(false);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public async Task<Models.Contract> GetContractAsync(string contractNumber, int version, int ukprn)
        {
            var contract = await _repository.GetContractAsync(contractNumber, version, ukprn).ConfigureAwait(false);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public async Task<ContractReminderResponse<IEnumerable<ContractReminderItem>>> GetContractRemindersAsync(int reminderInterval, int pageNumber, int pageSize, ContractSortOptions sort, SortDirection order, string templatedQueryString)
        {
            DateTime currentDateTimeMinusNumberOfDays = DateTime.UtcNow.Date.AddDays(-reminderInterval).AddHours(23).AddMinutes(59);

            _logger.LogInformation($"Get contract reminder by reminder interval : {reminderInterval} and cut off datetime {currentDateTimeMinusNumberOfDays}. - Current utc: {DateTime.UtcNow}");

            var contracts = await _repository.GetContractRemindersAsync(currentDateTimeMinusNumberOfDays, pageNumber, pageSize, sort, order);

            var metadata = new Metadata
            {
                TotalCount = contracts.TotalCount,
                PageSize = contracts.PageSize,
                CurrentPage = contracts.CurrentPage,
                TotalPages = contracts.TotalPages,
                HasNextPage = contracts.HasNextPage,
                HasPreviousPage = contracts.HasPreviousPage,
                NextPageUrl = contracts.HasNextPage ? _uriService.GetUri(SetPageValue(templatedQueryString, pageNumber + 1)).ToString() : string.Empty,
                PreviousPageUrl = contracts.HasPreviousPage ? _uriService.GetUri(SetPageValue(templatedQueryString, pageNumber - 1)).ToString() : string.Empty
            };

            var contractReminders = _mapper.Map<IEnumerable<ContractReminderItem>>(contracts.Items);

            var apiResponse = new ContractReminderResponse<IEnumerable<ContractReminderItem>>(contractReminders)
            {
                Paging = metadata
            };

            return apiResponse;
        }

        /// <inheritdoc/>
        public async Task<Contract> UpdateLastEmailReminderSentAndLastUpdatedAtAsync(UpdateLastEmailReminderSentRequest request)
        {
            var contract = await _repository.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(request.Id);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> ConfirmApprovalAsync(UpdateConfirmApprovalRequest request)
        {
            _logger.LogInformation($"[{nameof(ConfirmApprovalAsync)}] called with contract number: {request.ContractNumber} and contract version {request.ContractVersion}.");

            var contract = await _repository.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            ContractStatus newContractStatus = ContractStatus.Approved;

            _contractValidator.Validate(contract, request);
            _contractValidator.ValidateStatusChange(contract, newContractStatus);

            var updatedContractStatusResponse = new UpdatedContractStatusResponse
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion,
                Ukprn = contract.Ukprn,
                Status = (ContractStatus)contract.Status,
                Action = ActionType.ContractConfirmApproval
            };

            contract.Status = (int)newContractStatus;

            await _contractDocumentService.UpsertOriginalContractXmlAsync(contract, request);
            await _repository.UpdateContractAsync(contract);

            updatedContractStatusResponse.NewStatus = (ContractStatus)contract.Status;
            await _auditService.TrySendAuditAsync(GetAudit(updatedContractStatusResponse));

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> ApproveManuallyAsync(ContractRequest request)
        {
            _logger.LogInformation($"[{nameof(ApproveManuallyAsync)}] called with contract number: {request.ContractNumber} and contract version {request.ContractVersion}.");
            UpdatedContractStatusResponse updatedContractStatusResponse = null;

            var manuallyApproved = true;
            var newContractStatus = ContractStatus.Approved;

            var contract = await _repository.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas | ContractDataEntityInclude.Content);

            var existing = await _repository.GetByContractNumberAsync(request.ContractNumber);

            _contractValidator.Validate(contract, request, c => c.ContractContent != null);
            _contractValidator.ValidateStatusChange(contract, newContractStatus, manuallyApproved);

            updatedContractStatusResponse = new UpdatedContractStatusResponse
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion,
                Ukprn = contract.Ukprn,
                Status = (ContractStatus)contract.Status,
                Action = ActionType.ContractManualApproval
            };

            var contractRefernce = contract.ContractContent.FileName.Replace(".pdf", string.Empty);
            var signer = $"hand and approved by DfE";
            var updatedDate = DateTime.UtcNow;
            var signedContractDocument = _documentService.AddSignedDocumentPage(contract.ContractContent.Content, contractRefernce, signer, updatedDate, manuallyApproved, (ContractFundingType)contract.FundingType);

            contract.ContractContent.Content = signedContractDocument;
            contract.ContractContent.Size = signedContractDocument.Length;

            contract.Status = (int)newContractStatus;
            contract.SignedOn = updatedDate;
            contract.SignedBy = signer;
            contract.SignedByDisplayName = signer;
            contract.WasManuallyApproved = manuallyApproved;

            await _contractDocumentService.UpsertOriginalContractXmlAsync(contract, request);

            await _repository.UpdateContractAsync(contract);

            updatedContractStatusResponse.NewStatus = (ContractStatus)contract.Status;

            await _mediator.Publish(updatedContractStatusResponse);

            // if there are existing contracts and at least one of them is not the current contract
            if (existing.Any())
            {
                existing = existing.Where(p => p.ContractVersion != contract.ContractVersion).ToList();
                await ReplaceContractsWithGivenStatuses(existing, new int[] { (int)ContractStatus.Approved, (int)ContractStatus.ApprovedWaitingConfirmation });
            }

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> WithdrawalAsync(UpdateContractWithdrawalRequest request)
        {
            _logger.LogInformation($"[{nameof(WithdrawalAsync)}] called with contract number: {request.ContractNumber} and contract version {request.ContractVersion}.");

            var contract = await _repository.GetByContractNumberAndVersionWithIncludesAsync(request.ContractNumber, request.ContractVersion, ContractDataEntityInclude.Datas);

            ContractStatus newContractStatus = request.WithdrawalType;

            _contractValidator.Validate(contract, request);
            _contractValidator.ValidateStatusChange(contract, newContractStatus);

            var updatedContractStatusResponse = new UpdatedContractStatusResponse
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion,
                Ukprn = contract.Ukprn,
                Status = (ContractStatus)contract.Status,
                Action = ActionType.ContractWithdrawal
            };

            contract.Status = (int)newContractStatus;

            await _contractDocumentService.UpsertOriginalContractXmlAsync(contract, request);
            await _repository.UpdateContractAsync(contract);

            updatedContractStatusResponse.NewStatus = (ContractStatus)contract.Status;

            await _mediator.Publish(updatedContractStatusResponse);

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task PrependSignedPageToDocumentAndSaveAsync(int contractId)
        {
            _logger.LogInformation($"[{nameof(PrependSignedPageToDocumentAndSaveAsync)}] called with contract Id: {contractId}.");

            var contract = await _repository.GetAsync(contractId);

            _logger.LogInformation($"Contract signed and pre-pending page to pdf document. Id: {contractId}, signedOnDate {contract.SignedOn.Value}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation($"Size of the document before adding contract signed page to {contract.ContractContent.FileName} is : {contract.ContractContent.Content.Length}");

            var signedContractDocument = _documentService.AddSignedDocumentPage(
                contract.ContractContent.Content,
                contract.ContractContent.FileName.Replace(".pdf", string.Empty),
                contract.SignedByDisplayName,
                contract.SignedOn.Value,
                false,
                (ContractFundingType)contract.FundingType);

            sw.Stop();

            contract.ContractContent.Content = signedContractDocument;
            contract.ContractContent.Size = signedContractDocument.Length;

            _logger.LogInformation($"Time taken for pre-pending the page to {contract.ContractContent.FileName} document:  {sw.Elapsed}");

            _logger.LogInformation($"Size of the document after adding the contract signed page to {contract.ContractContent.FileName} is : {contract.ContractContent.Content.Length}");


            await _repository.UpdateContractAsync(contract);
        }

        private AuditModels.Audit GetAudit(UpdatedContractStatusResponse updatedContractStatusResponse)
        {
            string oldStatusName = updatedContractStatusResponse.Status.ToString("G");
            string newStatusName = updatedContractStatusResponse.NewStatus.ToString("G");

            string message = $"Contract [{updatedContractStatusResponse.ContractNumber}] Version number [{updatedContractStatusResponse.ContractVersion}] with Id [{updatedContractStatusResponse.Id}] has been {newStatusName}. Additional Information Details: ContractId is: {updatedContractStatusResponse.Id}. Contract Status Before was {oldStatusName} . Contract Status After is {newStatusName}";
            return new AuditModels.Audit()
            {
                Action = ActionType.ContractConfirmApproval,
                Severity = SeverityLevel.Information,
                Ukprn = updatedContractStatusResponse.Ukprn,
                Message = message,
                User = $"[{_appName}]"
            };
        }

        /// <summary>
        /// Retrieve the description of an enum value.
        /// </summary>
        /// <typeparam name="T">Enum type to validate.</typeparam>
        /// <param name="value">Enum value.</param>
        /// <returns>returns the description of the value provided.</returns>
        private string GetEnumDescription<T>(int value)
        {
            return Enum.Parse(typeof(T), value.ToString()).ToString();
        }

        /// <summary>
        /// Replace the templated page with page number.
        /// </summary>
        /// <param name="templatedQueryString">templated query string.</param>
        /// <param name="pageValue">page number.</param>
        /// <returns>Returns formatted url.</returns>
        private string SetPageValue(string templatedQueryString, int pageValue)
        {
            return templatedQueryString.Replace("{page}", pageValue.ToString());
        }

        /// <summary>
        /// Sets the statues of contracts to replaced where they match the given list of statuses.
        /// </summary>
        /// <param name="previousContracts">List of contracts to update.</param>
        /// <param name="replaceableStatuses">list of statuses to filter by.</param>
        /// <returns>Async Task.</returns>
        private async Task ReplaceContractsWithGivenStatuses(IEnumerable<Repository.DataModels.Contract> previousContracts, int[] replaceableStatuses)
        {
            if (previousContracts.Any(p => replaceableStatuses.Contains(p.Status)))
            {
                int ukprn = previousContracts.First().Ukprn;

                // Contracts found
                var listToUpdate = previousContracts.Where(p => replaceableStatuses.Contains(p.Status)).ToList();

                _logger.LogInformation($"[{nameof(ReplaceContractsWithGivenStatuses)}] found [{listToUpdate.Count}] contracts that need the status set to replaced.");
                foreach (var item in listToUpdate)
                {
                    _logger.LogInformation($"[{nameof(ReplaceContractsWithGivenStatuses)}] Replacing contract [{item.ContractNumber}-{item.ContractVersion}] with Id [{item.Id}]");

                    ContractStatus requiredContractStatus = (ContractStatus)item.Status;
                    ContractStatus newContractStatus = ContractStatus.Replaced;
                    var response = await _repository.UpdateContractStatusAsync(item.Id, requiredContractStatus, newContractStatus);

                    string msg = $"Contract [{response.ContractNumber}-{response.ContractVersion}] with Id [{response.Id}] has been replaced. " +
                        $"The contract status before was {response.Status.ToString("G")}. " +
                        $"The contract status after is {response.NewStatus.ToString("G")}.";

                    await _auditService.TrySendAuditAsync(new AuditModels.Audit()
                    {
                        Action = ActionType.ContractReplaced,
                        Severity = SeverityLevel.Information,
                        Ukprn = ukprn,
                        Message = msg,
                        User = _appName
                    });
                }
            }
        }
    }
}