using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/batches/{batchId}/curriculum")]
    public class CurriculumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurriculumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurriculumForBatch(int batchId)
        {
            var query = new GetCurriculumByBatchIdQuery { BatchId = batchId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCurriculum(int batchId, [FromBody] CreateCurriculumDto dto)
        {
            var command = new CreateCurriculumCommand { BatchId = batchId, CreateCurriculumDto = dto };
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return CreatedAtAction(nameof(GetCurriculumForBatch), new { batchId = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurriculum(int batchId, int id, [FromBody] UpdateCurriculumDto dto)
        {
            var command = new UpdateCurriculumCommand { Id = id, BatchId = batchId, UpdateCurriculumDto = dto };
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return NotFound(result);
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurriculum(int batchId, int id)
        {
            var command = new DeleteCurriculumCommand { Id = id, BatchId = batchId };
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return NotFound(result);
            return NoContent();
        }
    }
}
