using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.BoPhases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoPhasesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BoPhasesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a single BO Phase record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBoPhaseCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Bulk create BO Phase records for a specific batch
        /// </summary>
        [HttpPost("batch/{batchId}")]
        public async Task<IActionResult> CreateBatch(
            [FromRoute] int batchId,
            [FromBody] List<AddBoPhaseForBatchDto> boPhases)
        {
            var command = new CreateBoPhaseByBatchCommand
            {
                BatchId = batchId,
                BoPhases = boPhases
            };

            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get BO Phase details for a specific batch
        /// </summary>
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetByBatch(int batchId)
        {
            var query = new GetBoPhaseDetailsByBatchQuery(batchId);
            var result = await _mediator.Send(query);
            
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Update a BO Phase record
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBoPhaseCommand command)
        {
            command.BoPhaseId = id;
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}