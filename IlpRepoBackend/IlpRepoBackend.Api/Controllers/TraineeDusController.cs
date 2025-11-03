using IlpRepoBackend.Application.Command.TraineeDus;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.TraineeDus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeDusController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TraineeDusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a single TraineeDu record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTraineeDuCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Bulk create TraineeDu records for a specific batch
        /// </summary>
        [HttpPost("batch/{batchId}")]
        public async Task<IActionResult> CreateBatch(
            [FromRoute] int batchId,
            [FromBody] List<AddTraineeDuForBatchDto> traineeDus)
        {
            var command = new CreateTraineeDuByBatchCommand
            {
                BatchId = batchId,
                TraineeDus = traineeDus
            };

            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get TraineeDu details for a specific batch
        /// </summary>
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetByBatch(int batchId)
        {
            var query = new GetTraineeDuDetailsByBatchQuery(batchId);
            var result = await _mediator.Send(query);
            
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update a TraineeDu record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTraineeDuCommand command)
        {
            command.TraineeDuId = id;
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}