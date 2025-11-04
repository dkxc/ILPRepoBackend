using IlpRepoBackend.Application.Command.Documents;
using IlpRepoBackend.Application.Dto.Documents;
using IlpRepoBackend.Application.Query.Documents;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all document types (for editing purposes)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeDto>>>> GetAllDocumentTypes()
        {
            var query = new GetAllDocumentTypesQuery();
            var result = await _mediator.Send(query);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Create a new document type with optional template file
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<CreatedDocumentTypeDto>>> CreateDocumentType([FromForm] CreateDocumentTypeWithFileDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new CreateDocumentTypeCommand(request.Name, request.TemplateFile, request.FileType);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing document type with optional template file replacement
        /// </summary>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<DocumentTypeDto>>> UpdateDocumentType(int id, [FromForm] UpdateDocumentTypeWithFileDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new UpdateDocumentTypeCommand(id, request.Name, request.TemplateFile, request.FileType);
            var result = await _mediator.Send(command);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }

    /// <summary>
    /// DTO for handling multipart form data with file upload
    /// </summary>
    public class CreateDocumentTypeWithFileDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Document type name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public IFormFile? TemplateFile { get; set; }
        
        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "File type cannot exceed 50 characters")]
        public string? FileType { get; set; }
    }

    /// <summary>
    /// DTO for handling multipart form data with file upload for updates
    /// </summary>
    public class UpdateDocumentTypeWithFileDto
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(100, ErrorMessage = "Document type name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public IFormFile? TemplateFile { get; set; }
        
        [System.ComponentModel.DataAnnotations.StringLength(50, ErrorMessage = "File type cannot exceed 50 characters")]
        public string? FileType { get; set; }
    }
}