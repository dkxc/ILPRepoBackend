using IlpRepoBackend.Application.Commands.Results;
using IlpRepoBackend.Application.Queries.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ResultsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Uploads trainee scores and feedback from a structured JSON payload.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadResults([FromBody] UploadResultsCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets scores for all trainees in a specific batch (Admin).
        /// </summary>
        [HttpGet("scores/batch/{batchId}")]
        public async Task<IActionResult> GetScoresByBatch(int batchId)
        {
            var query = new GetScoresByBatchQuery { BatchId = batchId };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets detailed results (scores and feedback) for a specific trainee (Admin).
        /// </summary>
        [HttpGet("trainee/{traineeId}")]
        public async Task<IActionResult> GetTraineeResults(int traineeId)
        {
            var query = new GetTraineeResultsQuery { TraineeId = traineeId };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }
    }
}