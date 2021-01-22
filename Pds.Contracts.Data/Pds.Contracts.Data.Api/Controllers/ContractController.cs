using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Core.Logging;
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
        /// A <see cref="Task{Contract}" /> representing the result of the asynchronous operation.
        /// </returns>
        /// <response code="404">No contract found for given identifier.</response>
        /// <response code="500">Application error, invalid operation attempted, please report this to developer.</response>
        /// <response code="503">Service is un-available, try after sometime may be the application has been flooded.</response>
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
        /// A <see cref="ActionResult{Contract}" /> representing the return value of the this operation.
        /// </returns>
        /// <response code="404">No contract can be found for given contract number and version number.</response>
        /// <response code="500">Application error, invalid operation attempted, please report this to developer.</response>
        /// <response code="503">Service is un-available, try after sometime may be the application has been flooded.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public ActionResult<Contract> GetByContractNumberAndVersion(string contractNumber, int versionNumber)
        {
            _logger.LogInformation($"Get a contract by contract number: {contractNumber} and version: {versionNumber}.");
            var contract = _contractService.GetByContractNumberAndVersion(contractNumber, versionNumber);
            if (contract is null)
            {
                return NotFound();
            }

            return contract;
        }
    }
}