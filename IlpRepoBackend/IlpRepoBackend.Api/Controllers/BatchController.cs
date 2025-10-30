using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Application.Query.Batchs;
using IlpRepoBackend.Application.Query.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BatchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BatchController(IMediator mediator)
        {
            _mediator = mediator;
        }
        ///// <summary>
        ///// Get all Batchs
        ///// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var batchs = await _mediator.Send(new GetBatchsQuery());
            return Ok(batchs);
        }

        ///// <summary>
        ///// Add New Batch
        ///// </summary>
        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CreateBatchCommand command)
        {
            var batch = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id = batch.Id }, batch);
        }

        ///// <summary>
        ///// Update Batch
        ///// </summary>

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBatchCommand command)
        {
            command.Id = id;
            var updatedBatch = await _mediator.Send(command);
            if (updatedBatch == null)
            {
                return NotFound();
            }
            return Ok(updatedBatch);
        }

        ///// <summary>
        ///// Delete Batch
        ///// </summary>
         

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteBatchCommand { Id = id });
            return result ? NoContent() : NotFound();
        }

        ///// <summary>
        ///// Get Batch by ID
        ///// </summary>
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var batch = await _mediator.Send(new GetBatchByIdQuery { Id = id });
            return Ok(batch);
        }
    }

}
