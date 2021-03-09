using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Contracts.Data.Common.CustomExceptionHandlers;
using Pds.Contracts.Data.Common.Enums;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Contracts.Data.Services.Responses;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Controllers
{
    /// <summary>
    /// The contract (data) API controller.
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public class ContractController : BaseApiController
    {
        private readonly ILoggerAdapter<ContractController> _logger;

        private readonly IContractService _contractService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="contractService">The contract service.</param>
        public ContractController(
            ILoggerAdapter<ContractController> logger,
            IContractService contractService)
        {
            _logger = logger;
            _contractService = contractService;
        }

        /// <summary>
        /// Gets contract by id.
        /// </summary>
        /// <param name="id">The unique identifier of contract.</param>
        /// <returns>
        /// A <see cref="Task{Contract}"/> representing the result of the asynchronous operation.
        /// </returns>
        /// <response code="404">No contract found for given identifier.</response>
        /// <response code="500">
        /// Application error, invalid operation attempted, please report this to developer.
        /// </response>
        /// <response code="503">
        /// Service is un-available, try after sometime may be the application has been flooded.
        /// </response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Contract>> Get(int id)
        {
            _logger.LogInformation($"Get by unique identifier called with id: {id}.");
            var contract = await _contractService.GetAsync(id);
            if (contract is null)
            {
                return NotFound();
            }

            return contract;
        }

        /// <summary>
        /// Gets the contract by contract number and version.
        /// </summary>
        /// <param name="contractNumber">The contract number.</param>
        /// <param name="versionNumber">The version number.</param>
        /// <returns>
        /// A <see cref="ActionResult{Contract}"/> representing the return value of the this operation.
        /// </returns>
        /// <response code="404">
        /// No contract can be found for given contract number and version number.
        /// </response>
        /// <response code="500">
        /// Application error, invalid operation attempted, please report this to developer.
        /// </response>
        /// <response code="503">
        /// Service is un-available, try after sometime may be the application has been flooded.
        /// </response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<Contract>> GetByContractNumberAndVersionAsync(string contractNumber, int versionNumber)
        {
            _logger.LogInformation($"Get a contract by contract number: {contractNumber} and version: {versionNumber}.");
            var contract = await _contractService.GetByContractNumberAndVersionAsync(contractNumber, versionNumber);
            if (contract is null)
            {
                return NotFound();
            }

            return contract;
        }

        /// <summary>
        /// Gets a list of unsigned contracts that are past their due date.
        /// </summary>
        /// <param name="reminderInterval">Interval in days.</param>
        /// <param name="page">The page number to return.</param>
        /// <param name="count">The number of records in the page.</param>
        /// <param name="sort">Sort parameters to apply.</param>
        /// <param name="order">The order in which to .</param>
        /// <returns>A list of contracts that are overdue.</returns>
        /// <response code="204">No contracts need reminders to be issued.</response>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpGet("/api/contractReminders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ContractReminderResponse<IEnumerable<ContractReminderItem>>>> GetContractReminders(
            int reminderInterval = 14,
            int page = 1,
            int count = 10,
            ContractSortOptions sort = ContractSortOptions.LastUpdatedAt,
            SortDirection order = SortDirection.Asc)
        {
            _logger.LogInformation($"Get contract reminders called with reminder interval: {reminderInterval},  page: {page}, count: {count}, sort: {sort}, order: {order}");

            // This creates a template of the query string so that ONLY the page can be replaced
            // further down the process.
            string routeTemplateUrl = Request.Path.Value + $"?reminderInterval={reminderInterval}&page={{page}}&count={count}&sort={sort}&order={order}";

            var results = await _contractService.GetContractRemindersAsync(reminderInterval, page, count, sort, order, routeTemplateUrl);

            if (results is null || (results.Paging.TotalCount > 0 && page > results.Paging.TotalPages))
            {
                return NotFound();
            }
            else if (results.Paging.TotalCount == 0)
            {
                return NoContent();
            }

            return Ok(results);
        }

        /// <summary>
        /// Update the LastEmailReminderSent and LastUpdatedAt for a provided id, contract number
        /// and contract version.
        /// </summary>
        /// <param name="request">
        /// An UpdateLastEmailReminderSentRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>A list of contracts that are overdue.</returns>
        /// <response code="204">No contracts need reminders to be issued.</response>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="404">Contract was not found for the provided contract id.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpPatch("/api/contractReminder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> UpdateLastEmailReminderSent(ContractRequest request)
        {
            _logger.LogInformation($"Update LastEmailReminderSent and LastUpdatedAt called with contract number: {request.ContractNumber}, contract Id: {request.Id} ");
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var result = await _contractService.UpdateLastEmailReminderSentAndLastUpdatedAtAsync(request);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            return StatusCode(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Update the contract status to Approved.
        /// </summary>
        /// <param name="request">
        /// An UpdateConfirmApprovalRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>Returns a void.</returns>
        /// <response code="204">Blob document has no content.</response>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="404">Contract was not found for the provided contract id.</response>
        /// <response code="412">Contract pre checks failed.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpPatch("/api/confirmApproval")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> ConfirmApprovalAsync(UpdateConfirmApprovalRequest request)
        {
            _logger.LogInformation($"[{nameof(ConfirmApprovalAsync)}] called with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id} ");
            if (!ModelState.IsValid)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] provided data model failed validation check.");

                return ValidationProblem();
            }

            try
            {
                await _contractService.ConfirmApprovalAsync(request);
            }
            catch (BlobException ex)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] BlobException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id} and fileName: {request.FileName}. The Error: {ex.Message}");
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (BlobNoContentException ex)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] ContractBlobNoContentException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id} and fileName: {request.FileName}. The Error: {ex.Message}");
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (ContractStatusException ex)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] ContractStatusException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status412PreconditionFailed);
            }
            catch (ContractNotFoundException ex)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] ContractNotFoundException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return Problem(statusCode: StatusCodes.Status404NotFound);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[{nameof(ConfirmApprovalAsync)}] Internal server exception has occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Update contract status only when it has a current status of PublishedToProvider to WithdrawByAgency or WithdrawByProvider.
        /// </summary>
        /// <param name="request">
        /// An UpdateContractWithdrawalRequest model containing id, contract number, contract version and withdrawal status.
        /// </param>
        /// <returns>A list of contracts that are overdue.</returns>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="404">Contract was not found for the provided contract id.</response>
        /// <response code="412">Contract pre checks failed.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpPatch("/api/withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> UpdateContractWithdrawalAsync(UpdateContractWithdrawalRequest request)
        {
            string methodName = "UpdateContractWithdrawalAsync";
            _logger.LogInformation($"[{methodName}] called with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id} ");
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            try
            {
                var result = await _contractService.UpdateContractWithdrawalAsync(request);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
            }
            catch (ContractStatusException ex)
            {
                _logger.LogError($"[{methodName}] ContractStatusException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return StatusCode(StatusCodes.Status412PreconditionFailed);
            }
            catch (ContractNotFoundException ex)
            {
                _logger.LogError($"[{methodName}] ContractNotFoundException occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return StatusCode(StatusCodes.Status404NotFound);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[{methodName}] Internal server exception has occurred for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}. The Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Create a new contract with the given details.
        /// </summary>
        /// <param name="request">A <see cref="CreateContractRequest"/> containing the details of the contract.</param>
        /// <returns>OK response if the contract was created successfully, error otherwise.</returns>
        /// <response code="201">Contract has been successfully created in the system.</response>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="409">Contract with the given number and version already exists.</response>
        /// <response code="412">Contract witha a higher version already exists.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> CreateContract(CreateContractRequest request)
        {
            _logger.LogInformation($"[{nameof(CreateContract)}] called with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}");

            if (!ModelState.IsValid)
            {
                _logger.LogError($"[{nameof(CreateContract)}] provided data model failed validation check.");

                return ValidationProblem();
            }

            try
            {
                await _contractService.CreateAsync(request);

                _logger.LogInformation($"[{nameof(CreateContract)}] successfully created contract [{request.ContractNumber}] with version [{request.ContractVersion}]");

                return new StatusCodeResult(StatusCodes.Status201Created);
            }
            catch (DuplicateContractException dc)
            {
                _logger.LogError(dc, $"[{nameof(CreateContract)}] " + dc.Message);

                return Problem(detail: dc.Message, statusCode: StatusCodes.Status409Conflict);
            }
            catch (ContractWithHigherVersionAlreadyExistsException hva)
            {
                _logger.LogError(hva, $"[{nameof(CreateContract)}] " + hva.Message);

                return Problem(detail: hva.Message, statusCode: StatusCodes.Status412PreconditionFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(CreateContract)}] raised an error when creating new contract record.");
                return Problem(statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update the contract status to Approved. Manual Approve.
        /// </summary>
        /// <param name="request">
        /// An ContractRequest model containing id, contract number and contract version.
        /// </param>
        /// <returns>A list of contracts that are overdue.</returns>
        /// <response code="204">No contracts need reminders to be issued.</response>
        /// <response code="400">One or more parameters supplied are not valid.</response>
        /// <response code="401">Supplied authorisation credentials are not valid.</response>
        /// <response code="404">Contract was not found for the provided contract id.</response>
        /// <response code="412">Contract pre checks failed.</response>
        /// <response code="500">Application error, invalid operation attempted.</response>
        /// <response code="503">Service is un-available, retry the operation later.</response>
        [HttpPatch("manualApprove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult> ManualApprove(ContractRequest request)
        {
            _logger.LogInformation($"[{nameof(ManualApprove)}] called with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id} ");
            if (!ModelState.IsValid)
            {
                _logger.LogError($"[{nameof(ManualApprove)}] provided data model failed validation check.");

                return ValidationProblem();
            }

            try
            {
                await _contractService.ApproveManuallyAsync(request);
            }
            catch (InvalidContractRequestException ex)
            {
                _logger.LogError(ex, $"[{nameof(ManualApprove)}] Invalid contract request exception with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}.");
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (ContractNotFoundException ex)
            {
                _logger.LogError(ex, $"[{nameof(ManualApprove)}] failed to find a contract with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}.");
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (ContractExpectationFailedException ex)
            {
                _logger.LogError(ex, $"[{nameof(ManualApprove)}] Contract expectation failed exception with contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}.");
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (ContractStatusException ex)
            {
                _logger.LogError(ex, $"[{nameof(ManualApprove)}] failed contract status expectation. For the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}.");
                return StatusCode(StatusCodes.Status412PreconditionFailed, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(ManualApprove)}] Un-expected exception for the contract number: {request.ContractNumber}, contract Version number: {request.ContractVersion}, contract Id: {request.Id}.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
    }
}