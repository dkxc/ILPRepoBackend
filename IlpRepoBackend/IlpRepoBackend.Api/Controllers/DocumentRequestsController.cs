using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;
        private readonly IDocumentRequestRepository _documentRequestRepository;

        public DocumentRequestsController(
            IMediator mediator, 
            IWebHostEnvironment environment,
            IDocumentRequestRepository documentRequestRepository)
        {
            _mediator = mediator;
            _environment = environment;
            _documentRequestRepository = documentRequestRepository;
        }

        /// <summary>
        /// Create a document request with optional file upload
        /// </summary>
        /// <param name="dto">Document request data including batch ID, document ID, and due date</param>
        /// <returns>Created document request details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DocumentRequestResponseDto>>> CreateDocumentRequest([FromForm] CreateDocumentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<DocumentRequestResponseDto>($"Validation failed: {errors}", 400));
            }

            string? fileUrl = null;

            // Handle file upload if provided
            if (dto.File != null && dto.File.Length > 0)
            {
                try
                {
                    fileUrl = await SaveFileAsync(dto.File);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ApiResponse<DocumentRequestResponseDto>($"File upload failed: {ex.Message}", 500));
                }
            }

            var command = new CreateDocumentRequestCommand(dto.BatchId, dto.DocumentId, dto.DueDate, fileUrl);
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update a document request with optional file replacement
        /// </summary>
        /// <param name="id">Document request ID</param>
        /// <param name="dto">Updated document request data</param>
        /// <returns>Updated document request details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DocumentRequestResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DocumentRequestResponseDto>>> UpdateDocumentRequest(
            int id, 
            [FromForm] UpdateDocumentRequestDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new ApiResponse<DocumentRequestResponseDto>("Document request ID mismatch between URL and request body", 400));
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<DocumentRequestResponseDto>($"Validation failed: {errors}", 400));
            }

            // Get existing document request to retrieve old file URL
            var existingRequest = await _documentRequestRepository.GetDocumentRequestWithDetailsAsync(id);
            if (existingRequest == null)
            {
                return NotFound(new ApiResponse<DocumentRequestResponseDto>($"Document request with id {id} not found", 404));
            }

            var oldFileUrl = existingRequest.FileUrl;
            string? newFileUrl = null;

            // Handle file upload/replacement if provided
            if (dto.File != null && dto.File.Length > 0)
            {
                try
                {
                    newFileUrl = await SaveFileAsync(dto.File);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ApiResponse<DocumentRequestResponseDto>($"File upload failed: {ex.Message}", 500));
                }
            }

            var command = new UpdateDocumentRequestCommand(
                dto.Id, 
                dto.BatchId, 
                dto.DocumentId, 
                dto.DueDate, 
                newFileUrl, 
                oldFileUrl, 
                dto.RemoveExistingFile);
            
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            // Delete old file if:
            // 1. New file was uploaded, OR
            // 2. RemoveExistingFile flag is true
            if ((newFileUrl != null || dto.RemoveExistingFile) && !string.IsNullOrEmpty(oldFileUrl))
            {
                try
                {
                    DeleteFile(oldFileUrl);
                }
                catch
                {
                    // Log error but don't fail the request
                }
            }

            return Ok(result);
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative URL
            return $"/uploads/documents/{uniqueFileName}";
        }

        private void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            var filePath = Path.Combine(
                _environment.WebRootPath ?? _environment.ContentRootPath,
                fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}