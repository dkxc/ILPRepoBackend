using IlpRepoBackend.Application.Command.Links;
using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Query.Links;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LinksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all link types (for dropdown population)
        /// </summary>
        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<List<LinkTypeDto>>>> GetLinkTypes()
        {
            var query = new GetAllLinkTypesQuery();
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Create a new link type (e.g., "GitHub", "Figma", "Jira")
        /// </summary>
        [HttpPost("types")]
        public async Task<ActionResult<ApiResponse<CreatedLinkTypeDto>>> CreateLinkType([FromBody] CreateLinkTypeDto linkTypeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateLinkTypeCommand(linkTypeDto.Name);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return CreatedAtAction(nameof(GetLinkTypes), new { }, result);

            return BadRequest(result);
        }

        /// <summary>
        /// Assign a link type as requirement to all projects in a batch
        /// </summary>
        [HttpPut("assign-to-batch")]
        public async Task<ActionResult<ApiResponse<bool>>> AssignLinkTypeToBatch([FromBody] AssignLinkTypeToBatchDto assignDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new AssignLinkTypeToBatchCommand(assignDto.BatchId, assignDto.LinkTypeId);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get link types required for a specific batch with submission statistics
        /// </summary>
        [HttpGet("batch/{batchId}")]
        public async Task<ActionResult<ApiResponse<List<BatchLinkTypeDto>>>> GetLinkTypesByBatchId(int batchId)
        {
            var query = new GetLinkTypesByBatchIdQuery(batchId);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }
}