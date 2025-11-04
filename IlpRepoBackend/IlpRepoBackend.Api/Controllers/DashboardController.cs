using IlpRepoBackend.Application.Query.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet] // Route is now just GET /api/profile
        public async Task<IActionResult> GetTraineeDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "User is not authenticated or user ID is invalid." });
            }

            var query = new GetTraineeDashboardQuery { UserId = userId };
            var result = await _mediator.Send(query);

            if (!result.Succeeded)
            {
                return NotFound(result);
            }

            // This now returns the full dashboard object under a single call
            return Ok(result.Data);
        }
    }
}
