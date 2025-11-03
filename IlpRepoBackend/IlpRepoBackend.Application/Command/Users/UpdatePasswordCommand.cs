using IlpRepoBackend.Application.Dto;
using MediatR;

namespace IlpRepoBackend.Application.Command.Users
{
    public class UpdatePasswordCommand : IRequest<UserDto>
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }

        public UpdatePasswordCommand(int userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }
    }
}