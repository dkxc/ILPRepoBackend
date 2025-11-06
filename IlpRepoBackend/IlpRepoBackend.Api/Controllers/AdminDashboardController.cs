using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Dto.AdminDashboard;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using IlpRepoBackend.Infrastructure.Repositories;
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
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;

        public AdminDashboardController(IMediator mediator, AppDbContext context, ITrainingScheduleRepository trainingScheduleRepository)
        {
            _mediator = mediator;
            _context = context;
            _trainingScheduleRepository = trainingScheduleRepository;
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
        [HttpPost("update-training-hours")]
        public async Task<IActionResult> UpdateTrainingHours([FromBody] UpdateTrainingHoursDto dto)
        {
            if (dto.Hours < 0 || dto.Hours > 24)
                return BadRequest("Hours must be between 0 and 24.");

            var existing = await _trainingScheduleRepository
                .GetByBatchAndDateAsync(dto.BatchId, dto.TrainingDate);

            if (existing != null)
            {
                existing.Hours = dto.Hours;
                existing.UpdatedAt = DateTime.UtcNow;

                await _trainingScheduleRepository.UpdateOrCreateAsync(existing);

                return Ok(new { message = "Training hours updated", updated = true });
            }

            var newSchedule = new TrainingSchedule
            {
                BatchId = dto.BatchId,
                TrainingDate = dto.TrainingDate,
                Hours = dto.Hours,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _trainingScheduleRepository.UpdateOrCreateAsync(newSchedule);

            return Ok(new { message = "Training hours created", updated = false });
        }
    }

}

