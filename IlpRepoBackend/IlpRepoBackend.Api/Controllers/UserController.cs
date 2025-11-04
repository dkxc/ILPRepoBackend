using IlpRepoBackend.Application.Command.Users;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Query.Users;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IlpRepoBackend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _mediator.Send(new GetUserQuery());
            return Ok(users);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(user);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var user = await _mediator.Send(new GetUserByUsernameQuery { Username = username });
            return Ok(user);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            var user = await _mediator.Send(command);
            user.Id = id;
            return Ok(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteUserCommand { Id = id });
            return result ? NoContent() : NotFound();
        }
        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            try
            {
                // Get user ID from JWT token claims in the controller
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "User is not authenticated" });
                }

                var command = new UpdatePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
                var result = await _mediator.Send(command);

                return Ok(new
                {
                    message = "Password updated successfully",
                    user = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating password" });
            }
        }

        [HttpGet("admins")]
        public async Task<ActionResult<List<UserDto>>> GetAllAdmins()
        {
            var result = await _mediator.Send(new GetAllAdmin());

            if (result == null || result.Count == 0)
                return NotFound("No admin users found.");

            return Ok(result);
        }
    }

    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
