using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        //public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        //{
        //    var user = await _mediator.Send(command);
        //    return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        //}
        ///// <summary>
        ///// Add New Trainee
        ///// </summary>

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTraineeCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        ///// <summary>
        ///// Add All Trainee
        ///// </summary>

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var trainees = await _mediator.Send(new Application.Query.Trainees.GetTraineeQuery());
            return Ok(trainees);
        }

        ///// <summary>
        ///// Get All Trainee By Batch Id
        ///// </summary>
        
        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetTraineesByBatchId(int batchId)
        {
            var trainees = await _mediator.Send(new Application.Query.Trainees.GetTraineesByBatchIdQuery(batchId));
            return Ok(trainees);
        }
        ///// <summary>
        ///// Add All Trainee By Batch Id
        ///// </summary>
        [HttpPost("batch/{batchId}")]
        public async Task<IActionResult> Create(
            [FromRoute] int batchId,
            [FromBody] List<AddTraineeForABatchDto> trainees)
        {
            var command = new CreateTraineeByBatch
            {
                BatchId = batchId,
                Trainees = trainees
            };

            var result = await _mediator.Send(command);

            if (result == null)
            {
                return BadRequest("Result is null");
            }

            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "success", data = result.Data });
        }

    }
}
