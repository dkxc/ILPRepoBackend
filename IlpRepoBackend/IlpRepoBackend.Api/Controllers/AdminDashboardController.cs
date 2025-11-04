using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AppDbContext _context;

        public AdminDashboardController(IMediator mediator, AppDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        // ✅ Dashboard summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _mediator.Send(new GetAdminDashboardSummaryQuery());
            return Ok(summary);
        }

        // ✅ Batch details by ID
        [HttpGet("batch-details/{batchId}")]
        public async Task<IActionResult> GetBatchDetails(int batchId)
        {
            var details = await _mediator.Send(new GetBatchDetailQuery(batchId));
            return Ok(details);
        }

        // ✅ List of all batches (for dropdown or other usage)
        [HttpGet("batches")]
        public async Task<IActionResult> GetAllBatchNames()
        {
            var query = new GetAllBatchNamesQuery();
            var result = await _mediator.Send(query);
            return Ok(result.Select(b => new
            {
                id = b.BatchId,
                name = b.BatchName
            }));
        }

        // ✅ Projects by Batch ID
        [HttpGet("projects/{batchId}")]
        public async Task<IActionResult> GetProjectsByBatchId(int batchId)
        {
            var query = new GetProjectsByBatchIdQuery(batchId);
            var results = await _mediator.Send(query);
            return Ok(results);
        }

        // ✅ NEW: Get all batch types (for dropdown in frontend)
        [HttpGet("batch-types")]
        public async Task<IActionResult> GetBatchTypes()
        {
            var batchTypes = await _context.BatchTypes
                .Select(bt => new { bt.Id, bt.Name })
                .ToListAsync();

            // Add “All Batch Types” as default option at the top
            var allOption = new[] { new { Id = 0, Name = "All Batch Types" } };
            return Ok(allOption.Concat(batchTypes));
        }

        // ✅ NEW: Training Hours Report API
        [HttpGet("training-hours-report")]
        public async Task<IActionResult> GetTrainingHoursReport(
    int? batchTypeId,
    DateTime startDate,
    DateTime endDate)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            var query = new GetTrainingHoursReportQuery(batchTypeId, startDate, endDate);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}
