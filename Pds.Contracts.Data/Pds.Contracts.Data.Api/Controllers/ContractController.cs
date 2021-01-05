using Microsoft.AspNetCore.Mvc;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Contracts.Data.Services.Models;
using Pds.Core.Logging;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Controllers
{
    /// <summary>
    /// The example controller.
    /// </summary>
    [ApiController]
    public class ContractController : BaseApiController
    {
        private readonly ILoggerAdapter<ContractController> _logger;
        private readonly IContractService _contractService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exampleService">The example service.</param>
        public ContractController(
            ILoggerAdapter<ContractController> logger,
            IContractService exampleService)
        {
            _logger = logger;
            _contractService = exampleService;
        }

        /// <summary>
        /// Get the example page.
        /// </summary>
        /// <param name="id">The unique identifier of contract.</param>
        /// <returns>
        /// A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<Contract> Get(int id)
        {
            _logger.LogInformation($"Get by unique identifier called with id: {id}.");
            return await _contractService.GetAsync(id);
        }
    }
}