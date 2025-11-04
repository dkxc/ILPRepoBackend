using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.Trainees;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineesController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public TraineesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTraineeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetAll), new { id = result.Data?.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetTraineeQuery());
            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }
        
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetTraineesByBatchId(int batchId)
        {
            var result = await _mediator.Send(new GetTraineesByBatchIdQuery(batchId));
            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get training details for a specific trainee
        /// </summary>
        /// <param name="traineeId">The trainee ID</param>
        /// <returns>Training details including batch, buddy, OJT mentor, DU allocation, and location</returns>
        [HttpGet("{traineeId}/training-details")]
        public async Task<IActionResult> GetTrainingDetails(int traineeId)
        {
            var query = new GetTraineeTrainingDetailsQuery(traineeId);
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Update training details for a specific trainee
        /// </summary>
        /// <param name="traineeId">The trainee ID</param>
        /// <param name="command">Training details to update</param>
        /// <returns>Updated training details</returns>
        [HttpPut("{traineeId}/training-details")]
        public async Task<IActionResult> UpdateTrainingDetails(int traineeId, [FromBody] UpdateTraineeTrainingDetailsCommand command)
        {
            command.TraineeId = traineeId;
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTraineeCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
                return BadRequest(result);
                
            return Ok(result);
        }

        [HttpPost("batch/{batchId}")]
        public async Task<IActionResult> CreateBatch(
            [FromRoute] int batchId,
            [FromBody] List<AddTraineeForABatchDto> trainees)
        {
            var command = new CreateTraineeByBatch
            {
                BatchId = batchId,
                Trainees = trainees
            };

            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
