using IlpRepoBackend.Application.Query.Phases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhasesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PhasesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all phases for a specific batch
        /// </summary>
        /// <param name="batchId">The batch ID</param>
        /// <returns>List of phases for the specified batch</returns>
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetPhasesByBatchId(int batchId)
        {
            var query = new GetPhasesByBatchIdQuery(batchId);
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }
    }
}
