using IlpRepoBackend.Application.Command.Attendance;
using IlpRepoBackend.Application.Dto.Attendance;
using IlpRepoBackend.Application.Query.Attendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AttendanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AttendanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("batch/{batchId}")]
        public async Task<IActionResult> GetBatchAttendance(int batchId, [FromQuery(Name = "start_date")] string? startDateStr, [FromQuery(Name = "end_date")] string? endDateStr)
        {
            var query = new GetAttendanceByBatchQuery { BatchId = batchId };

            if (DateOnly.TryParse(startDateStr, CultureInfo.InvariantCulture, out var startDate))
                query.StartDate = startDate;

            if (DateOnly.TryParse(endDateStr, CultureInfo.InvariantCulture, out var endDate))
                query.EndDate = endDate;

            var result = await _mediator.Send(query);
            return Ok(result.Data);
        }

        [HttpPut("batch/{batchId}")]
        public async Task<IActionResult> UpdateBatchAttendance(int batchId, [FromBody] UpdateBatchAttendanceDto updateDto)
        {
            var command = new UpdateBatchAttendanceCommand
            {
                BatchId = batchId,
                UpdateData = updateDto
            };
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { status = "success", updateRecordCount = result.Data });
        }

        [HttpPost("batch/{batchId}/upload")]
        public async Task<IActionResult> UploadAttendance(int batchId, [FromBody] List<UploadAttendanceDto> uploadData)
        {
            var command = new UploadBatchAttendanceCommand
            {
                BatchId = batchId,
                UploadData = uploadData
            };
            var result = await _mediator.Send(command);
            if (!result.Succeeded) return BadRequest(result);
            return Ok(new { status = "success", message = result.Data });
        }
    }
}
