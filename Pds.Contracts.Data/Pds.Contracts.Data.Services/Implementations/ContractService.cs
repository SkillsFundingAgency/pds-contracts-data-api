using AutoMapper;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Common.Responses;
using Pds.Contracts.Data.Repository.Interfaces;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public ContractService(
            IContractRepository repository,
            IMapper mapper,
            IUriService uriService,
            ILoggerAdapter<ContractService> logger,
            IAuditService auditService,
            ISemaphoreOnEntity<string> semaphoreOnEntity,
            IDocumentManagementContractService documentService,
            IContractValidationService contractValidator)
        {
            _repository = repository;
            _mapper = mapper;
            _uriService = uriService;
            _logger = logger;
            _auditService = auditService;
            _semaphoreOnEntity = semaphoreOnEntity;
            _documentService = documentService;
            _contractValidator = contractValidator;
        }

        /// <inheritdoc/>
        public async Task CreateAsync(CreateContractRequest request)
        {
            await _semaphoreOnEntity.WaitAsync(request.ContractNumber).ConfigureAwait(false);

            try
            {
                _logger.LogInformation($"Creating new contract [{request.ContractNumber}] version [{request.ContractVersion}] for [{request.UKPRN}].");

                await ValidateForNewContractAsync(request.ContractNumber, request.ContractVersion);

                var newContract = _mapper.Map<Repository.DataModels.Contract>(request);
                newContract.LastUpdatedAt = newContract.CreatedAt = DateTime.UtcNow;

                await _repository.CreateAsync(newContract);

                string message = $"Contract [{newContract.ContractNumber}] version [{newContract.ContractVersion}] has been created.  The contract status after is Ready to sign .";

                await _auditService.TrySendAuditAsync(
                    new Audit.Api.Client.Models.Audit()
                    {
                        Action = ActionType.ContractCreated,
                        Severity = SeverityLevel.Information,
                        Ukprn = newContract.Ukprn,
                        Message = message,
                        User = $"[{_appName}]"
                    });

                _logger.LogInformation($"Contract [{newContract.ContractNumber}] version [{newContract.ContractVersion}] has been created for [{newContract.Ukprn}].");
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
        public async Task<Contract> UpdateLastEmailReminderSentAndLastUpdatedAtAsync(ContractRequest request)
        {
            var contract = await _repository.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(request.Id);
            return _mapper.Map<Models.Contract>(contract);
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> UpdateContractConfirmApprovalAsync(UpdateConfirmApprovalRequest request)
        {
            _logger.LogInformation($"[{nameof(UpdateContractConfirmApprovalAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} ");

            var contract = await _repository.GetAsync(request.Id);
            _contractValidator.Validate(contract, request);

            ContractStatus requiredContractStatus = ContractStatus.ApprovedWaitingConfirmation;
            ContractStatus newContractStatus = ContractStatus.Approved;
            var updatedContractStatusResponse = await _repository.UpdateContractStatusAsync(request.Id, requiredContractStatus, newContractStatus);

            await _auditService.TrySendAuditAsync(GetAudit(updatedContractStatusResponse));

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> ApproveManuallyAsync(ContractRequest request)
        {
            _logger.LogInformation($"[{nameof(ApproveManuallyAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id}.");
            UpdatedContractStatusResponse updatedContractStatusResponse = null;

            var manullyApproved = true;
            var newContractStatus = ContractStatus.Approved;

            var contract = await _repository.GetContractWithContractContentAsync(request.Id);

            _contractValidator.Validate(contract, request, c => c.ContractContent != null);
            _contractValidator.ValidateStatusChange(contract, newContractStatus, manullyApproved);

            updatedContractStatusResponse = new UpdatedContractStatusResponse
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion,
                Ukprn = contract.Ukprn,
                Status = contract.Status
            };

            var contractRefernce = contract.ContractContent.FileName.Replace(".pdf", string.Empty);
            var signer = $"hand and approved by ESFA";
            var updatedDate = DateTime.UtcNow;
            var signedContractDocument = _documentService.AddSignedDocumentPage(contract.ContractContent.Content, contractRefernce, signer, updatedDate, manullyApproved, (ContractFundingType)contract.FundingType);

            contract.ContractContent.Content = signedContractDocument;
            contract.ContractContent.Size = signedContractDocument.Length;

            contract.Status = (int)newContractStatus;
            contract.SignedOn = updatedDate;
            contract.SignedBy = signer;
            contract.SignedByDisplayName = signer;
            contract.WasManuallyApproved = manullyApproved;

            await _repository.UpdateContractAsync(contract);

            updatedContractStatusResponse.NewStatus = contract.Status;
            await _auditService.TrySendAuditAsync(GetAudit(updatedContractStatusResponse));

            return updatedContractStatusResponse;
        }

        /// <inheritdoc/>
        public async Task<UpdatedContractStatusResponse> UpdateContractWithdrawalAsync(UpdateContractWithdrawalRequest request)
        {
            _logger.LogInformation($"[{nameof(UpdateContractWithdrawalAsync)}] called with contract number: {request.ContractNumber}, contract Id: {request.Id} ");

            var contract = await _repository.GetAsync(request.Id);
            _contractValidator.Validate(contract, request);

            var updatedContractStatusResponse = await _repository.UpdateContractStatusAsync(request.Id, ContractStatus.PublishedToProvider, request.WithdrawalType);
            await _auditService.TrySendAuditAsync(GetAudit(updatedContractStatusResponse));

            return updatedContractStatusResponse;
        }

        private AuditModels.Audit GetAudit(UpdatedContractStatusResponse updatedContractStatusResponse)
        {
            string oldStatusName = GetEnumDescription<ContractStatus>(updatedContractStatusResponse.Status);
            string newStatusName = GetEnumDescription<ContractStatus>(updatedContractStatusResponse.NewStatus);

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

        private async Task ValidateForNewContractAsync(string contractNumber, int contractVersion)
        {
            var existing = await _repository.GetByContractNumberAsync(contractNumber);
            if (existing?.Any(p => p.ContractVersion > contractVersion) == true)
            {
                throw new ContractWithHigherVersionAlreadyExistsException(contractNumber, contractVersion);
            }
            else if (existing?.Any(p => p.ContractVersion == contractVersion) == true)
            {
                throw new DuplicateContractException(contractNumber, contractVersion);
            }
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
    }
}