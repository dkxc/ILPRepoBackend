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
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        /// Get project details by ID - includes project name, status, progress, technology stack, trainees with name and email, and project links with link type name
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> GetProjectDetailsById(int id)
        {
            var query = new GetProjectDetailsByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return NotFound(result);
        }

        /// <summary>
        /// Update project technology stack
        /// </summary>
        [HttpPut("{id}/technology")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateProjectTechnology(int id, [FromBody] UpdateTechnologyDto technologyDto)
        {
            var command = new UpdateProjectTechnologyCommand(id, technologyDto.Technology);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Add or update a project link - automatically creates new or updates existing based on link type
        /// </summary>
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProjectDto updateProjectDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var command = new UpdateProjectCommand(id, updateProjectDto);
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "Project updated successfully", data = result.Data });
        }

        /// <summary>
        /// Delete project
        /// </summary>
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteProjectCommand(id);
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "Project deleted successfully" });
        }

        [HttpPut("{id}/links")]
        public async Task<ActionResult<ApiResponse<bool>>> UpsertProjectLink(int id, [FromBody] AddProjectLinkDto linkData)
        {
            var command = new UpsertProjectLinkCommand(id, linkData.LinkId, linkData.LinkUrl);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }
}