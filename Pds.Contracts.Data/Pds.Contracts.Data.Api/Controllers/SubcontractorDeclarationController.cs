using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Contracts.Data.Repository.DataModels;
using Pds.Contracts.Data.Services.Interfaces;
using Pds.Core.Logging;
using System.Threading.Tasks;

namespace Pds.Contracts.Data.Api.Controllers
{
    /// <summary>
    /// The Subcontractor Declaration API Controller.
    /// </summary>
    [Produces("application/json")]
    [Route("api/SubcontractorDeclaration")]
    [ApiController]
    public class SubcontractorDeclarationController : BaseApiController
    {
        private readonly ILoggerAdapter<SubcontractorDeclarationController> _logger;

        private readonly ISubcontractorDeclarationService _subcontractorDeclarationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcontractorDeclarationController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="subcontractorDeclarationService">Subcontractor declaration service.</param>
        public SubcontractorDeclarationController(
            ILoggerAdapter<SubcontractorDeclarationController> logger,
            ISubcontractorDeclarationService subcontractorDeclarationService)
        {
            _logger = logger;
            _subcontractorDeclarationService = subcontractorDeclarationService;
        }

        /// <summary>
        /// Gets full subcontractor declaration by id.
        /// </summary>
        /// <param name="id">The unique identifier of full subcontractor declaration.</param>
        /// <returns>
        /// A <see cref="Task{Contract}"/> representing the result of the asynchronous operation.
        /// </returns>
        /// <response code="404">No full subcontractor declaration found for given identifier.</response>
        /// <response code="500">
        /// Application error, invalid operation attempted, please report this to developer.
        /// </response>
        /// <response code="503">
        /// Service is un-available, try after sometime may be the application has been flooded.
        /// </response>
        [HttpGet("GetFullSubcontractorDeclarationById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<FullSubcontractorDeclaration>> GetFullSubcontractorDeclarationById(int id)
        {
            _logger.LogInformation($"Get full subcontractor declaration by id: {id}.");
            var subcontractorDeclaration = await _subcontractorDeclarationService.GetFullSubcontractorDeclarationById(id);
            if (subcontractorDeclaration is null)
            {
                return NotFound();
            }

            return subcontractorDeclaration;
        }
    }
}
