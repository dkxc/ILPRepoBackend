using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Application.Wrapper;

//using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IlpRepoBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto projectDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateProjectCommand(projectDto);
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return CreatedAtAction(
                    actionName: "GetById", // name of the GET method for this resource
                    routeValues: new { id = result.Data.Id }, // route parameters for that action
                    value: new { message = "Project created successfully", data = result.Data } // response body
);

        }
        /// <summary>
        /// Create multiple projects for a single batch
        /// </summary>
        [HttpPost("batch")]
        // [Authorize] // Remove or comment this out
        public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> CreateBatchProjects(
            [FromBody] CreateBatchProjectsDto batchProjectsDto)
        {
            var command = new CreateBatchProjectsCommand(batchProjectsDto);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetAll(
            [FromQuery] int? batchId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? technology = null)
        {
            var query = new GetAllProjectsQuery(batchId, status, technology);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get a single project by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProjectDto>>> GetById(int id)
        {
            var query = new GetProjectByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return NotFound(result);
        }

        /// <summary>
        /// Update project
        /// </summary>
        

    }
}