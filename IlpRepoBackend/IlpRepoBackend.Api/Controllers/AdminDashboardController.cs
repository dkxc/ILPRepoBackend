using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.AdminDashboard;



namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _mediator.Send(new GetAdminDashboardSummaryQuery());
            return Ok(summary);
        }
        [HttpGet("batch-details/{batchId}")]
        public async Task<IActionResult> GetBatchDetails(int batchId)
        {
            var details = await _mediator.Send(new GetBatchDetailQuery(batchId));
            return Ok(details);
        }

        [HttpGet("batches")]
        public async Task<IActionResult> GetAllBatchNames()
        {
            var query = new GetAllBatchNamesQuery();
            var result = await _mediator.Send(query);
            return Ok(result.Select(b => new
            {
                id = b.BatchId,
                name = b.BatchName // ✅ must be BatchName, not Name
            }));
        }
        [HttpGet("projects/{batchId}")]
        public async Task<IActionResult> GetProjectsByBatchId(int batchId)
        {
            var query = new GetProjectsByBatchIdQuery(batchId);
            var results = await _mediator.Send(query);
            return Ok(results);
        }

    }
}
