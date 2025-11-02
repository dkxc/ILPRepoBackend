using IlpRepoBackend.Application.Command.DocumentRequests;
using IlpRepoBackend.Application.Dto.DocumentRequests;
using IlpRepoBackend.Application.Query.DocumentRequests;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentRequirementsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentRequirementsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get document requirements for a batch
        /// </summary>
        [HttpGet("batch/{batchId}")]
        public async Task<ActionResult<ApiResponse<List<DocumentRequirementResponseDto>>>> GetDocumentRequirementsByBatchId(int batchId)
        {
            var query = new GetDocumentRequirementsByBatchIdQuery(batchId);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Get document requirements for a project (for dropdown selection by project leads)
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<List<ProjectDocumentRequirementDto>>>> GetDocumentRequirementsByProjectId(int projectId)
        {
            var query = new GetDocumentRequirementsByProjectIdQuery(projectId);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Set document requirement for a batch (creates requirement for all projects in the batch)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<DocumentRequirementResponseDto>>> SetDocumentRequirement([FromBody] SetDocumentRequirementDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new SetDocumentRequirementCommand(request.DocumentTypeId, request.BatchId, request.DueDate);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Update document requirement deadline for a batch (updates all projects in the batch)
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<ApiResponse<DocumentRequirementResponseDto>>> UpdateDocumentRequirement([FromBody] UpdateDocumentRequirementDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new UpdateDocumentRequirementCommand(request.DocumentTypeId, request.BatchId, request.DueDate);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Delete document requirement for a batch (removes requirement from all projects in the batch)
        /// </summary>
        [HttpDelete]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocumentRequirement([FromQuery] int documentTypeId, [FromQuery] int batchId)
        {
            var command = new DeleteDocumentRequirementCommand(documentTypeId, batchId);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }
}