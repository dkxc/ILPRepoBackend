using IlpRepoBackend.Application.Command.BatchTypes;
using IlpRepoBackend.Application.Query.BatchTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BatchTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetBatchTypesQuery());
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBatchTypeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result == null) return BadRequest();
            if (!result.Succeeded) return BadRequest(new { message = result.Message });
            return CreatedAtAction(nameof(GetAll), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBatchTypeCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            if (result == null) return BadRequest();
            if (!result.Succeeded) return BadRequest(new { message = result.Message });
            return Ok(result);
        }
    }
}
