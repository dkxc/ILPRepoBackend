using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentSubmissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;

        public DocumentSubmissionsController(
            IMediator mediator, 
            IWebHostEnvironment environment,
            IDocumentSubmissionRepository documentSubmissionRepository)
        {
            _mediator = mediator;
            _environment = environment;
            _documentSubmissionRepository = documentSubmissionRepository;
        }

        /// <summary>
        /// Upload a document submission with file
        /// </summary>
        /// <param name="dto">Document submission data including request ID, document ID, and file</param>
        /// <returns>Created document submission details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DocumentSubmissionDto>>> UploadDocumentSubmission([FromForm] UploadDocumentSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<DocumentSubmissionDto>($"Validation failed: {errors}", 400));
            }

            // Validate file
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new ApiResponse<DocumentSubmissionDto>("File is required", 400));
            }

            string submissionLink;
            string fileName = dto.File.FileName;
            string fileType = GetFileType(dto.File.FileName);

            try
            {
                // Save file to server
                submissionLink = await SaveFileAsync(dto.File);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DocumentSubmissionDto>($"File upload failed: {ex.Message}", 500));
            }

            // Create command with file information
            var command = new UploadDocumentSubmissionCommand(
                dto.RequestId,
                dto.DocumentId,
                dto.TraineeId,
                submissionLink,
                fileName,
                fileType
            );

            var result = await _mediator.Send(command);

            // If submission failed, delete the uploaded file
            if (!result.Success && !string.IsNullOrEmpty(submissionLink))
            {
                try
                {
                    DeleteFile(submissionLink);
                }
                catch { }
            }

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get document submission details including download link
        /// </summary>
        /// <param name="id">Document submission ID</param>
        /// <returns>Document submission details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DocumentSubmissionDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DocumentSubmissionDto>>> GetDocumentSubmission(int id)
        {
            var query = new GetDocumentSubmissionQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a document submission and its associated file
        /// </summary>
        /// <param name="id">Document submission ID</param>
        /// <returns>Confirmation of deletion</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDocumentSubmission(int id)
        {
            // Get submission to retrieve file path before deletion
            var submission = await _documentSubmissionRepository.GetByIdAsync(id);
            string? filePathToDelete = null;

            if (submission != null && !string.IsNullOrEmpty(submission.SubmissionLink))
            {
                filePathToDelete = Path.Combine(
                    _environment.WebRootPath ?? _environment.ContentRootPath,
                    submission.SubmissionLink.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            // Delete from database
            var command = new DeleteDocumentSubmissionCommand(id);
            var result = await _mediator.Send(command);

            // If database deletion was successful, delete the physical file
            if (result.Success && !string.IsNullOrEmpty(filePathToDelete))
            {
                try
                {
                    if (System.IO.File.Exists(filePathToDelete))
                    {
                        System.IO.File.Delete(filePathToDelete);
                    }
                }
                catch
                {
                    // Log error but don't fail the request since DB deletion succeeded
                }
            }

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        private async Task<string> SaveFileAsync(IFormFile file)
        {
            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "submissions");
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
            return $"/uploads/submissions/{uniqueFileName}";
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

        private string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            return extension switch
            {
                ".pdf" => "PDF",
                ".doc" or ".docx" => "Word",
                ".xls" or ".xlsx" => "Excel",
                ".ppt" or ".pptx" => "PowerPoint",
                ".txt" => "Text",
                ".jpg" or ".jpeg" or ".png" or ".gif" => "Image",
                ".zip" or ".rar" => "Archive",
                _ => "Unknown"
            };
        }
    }
}