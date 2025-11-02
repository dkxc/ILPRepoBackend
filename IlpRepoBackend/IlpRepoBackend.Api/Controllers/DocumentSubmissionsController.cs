using IlpRepoBackend.Application.Command.DocumentSubmissions;
using IlpRepoBackend.Application.Dto.DocumentSubmissions;
using IlpRepoBackend.Application.Query.DocumentSubmissions;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/submitted-documents")]
    public class SubmittedDocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubmittedDocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get document submissions for a project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<List<ProjectDocumentSubmissionInfo>>>> GetByProjectId(int projectId)
        {
            var query = new GetDocumentSubmissionsByProjectIdQuery(projectId);
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Submit a document for a document request
        /// </summary>
        [HttpPost("submit")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<DocumentSubmissionResultDto>>> SubmitDocument([FromForm] SubmitDocumentDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new SubmitDocumentCommand(request.DocumentRequestId, request.DocumentFile);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }
}