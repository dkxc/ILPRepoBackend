using IlpRepoBackend.Application.Command.PhaseTypes;
using IlpRepoBackend.Application.Query.PhaseTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhaseTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PhaseTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all phase types
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetPhaseTypesQuery());
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get phase type by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetPhaseTypeByIdQuery { Id = id });
            if (result == null) return NotFound();
            if (!result.Succeeded) return NotFound(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Create new phase type
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePhaseTypeCommand command)
        {
            var result = await _mediator.Send(command);
            if (result == null) return BadRequest();
            if (!result.Succeeded) return BadRequest(new { message = result.Message });
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Update phase type
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhaseTypeCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            if (result == null) return BadRequest();
            if (!result.Succeeded) return BadRequest(new { message = result.Message });
            return Ok(result);
        }

        /// <summary>
        /// Delete phase type
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeletePhaseTypeCommand { Id = id });
            return result ? NoContent() : NotFound();
        }
    }
}