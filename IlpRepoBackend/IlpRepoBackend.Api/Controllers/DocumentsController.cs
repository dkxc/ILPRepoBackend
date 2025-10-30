using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IlpRepoBackend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all document types/names for dropdown population
        /// </summary>
        /// <returns>List of all document types</returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentTypeDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentTypeDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeDto>>>> GetAllDocumentTypes()
        {
            var query = new GetAllDocumentTypesQuery();
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get document template link for downloading
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>Document details including template link</returns>
        [HttpGet("{id}/link")]
        [ProducesResponseType(typeof(ApiResponse<DocumentLinkDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentLinkDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<DocumentLinkDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DocumentLinkDto>>> GetDocumentLink(int id)
        {
            var query = new GetDocumentLinkQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            return Ok(result);
        }
    }
}