using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get project details including project name, batch names, trainee list, number of trainees, tech stack, and project links
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> GetProjectDetails(int id)
        {
            var query = new GetProjectDetailsQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get project tech stack details by project ID
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project tech stack details</returns>
        [HttpGet("{id}/tech-stack")]
        [ProducesResponseType(typeof(ApiResponse<ProjectTechStackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProjectTechStackDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProjectTechStackDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProjectTechStackDto>>> GetProjectTechStack(int id)
        {
            var query = new GetProjectTechStackQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update project links
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="updateDto">Update data containing project links</param>
        /// <returns>Updated project details</returns>
        [HttpPut("{id}/links")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> UpdateProjectLinks(
            int id, 
            [FromBody] UpdateProjectLinksDto updateDto)
        {
            if (id != updateDto.ProjectId)
            {
                return BadRequest(new ApiResponse<ProjectDetailsDto>("Project ID mismatch between URL and request body", 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<ProjectDetailsDto>($"Validation failed: {errors}", 400));
            }

            var command = new UpdateProjectLinksCommand(updateDto.ProjectId, updateDto.TraineeId, updateDto.ProjectLinks);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update project technology stack
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="updateDto">Update data containing technology stack</param>
        /// <returns>Updated project details</returns>
        [HttpPut("{id}/tech-stack")]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<ProjectDetailsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProjectDetailsDto>>> UpdateProjectTechStack(
            int id, 
            [FromBody] UpdateProjectTechStackDto updateDto)
        {
            if (id != updateDto.ProjectId)
            {
                return BadRequest(new ApiResponse<ProjectDetailsDto>("Project ID mismatch between URL and request body", 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<ProjectDetailsDto>($"Validation failed: {errors}", 400));
            }

            var command = new UpdateProjectTechStackCommand(updateDto);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Remove a teammate (trainee) from a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="traineeId">Trainee ID to remove from the project</param>
        /// <returns>Confirmation of removal</returns>
        [HttpDelete("{id}/teammates/{traineeId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveTeammate(int id, int traineeId)
        {
            var command = new RemoveTeammateCommand(id, traineeId);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}
