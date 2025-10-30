using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LinksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all link types for dropdown population
        /// </summary>
        /// <returns>List of all link types</returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<List<LinkTypeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LinkTypeDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<LinkTypeDto>>>> GetAllLinkTypes()
        {
            var query = new GetAllLinkTypesQuery();
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}