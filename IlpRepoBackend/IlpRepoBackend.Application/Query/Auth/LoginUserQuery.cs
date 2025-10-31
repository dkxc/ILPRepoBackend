using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Query.Auth
{
    public class LoginUserQuery : IRequest<ApiResponse<LoginResponseDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }


        public LoginUserQuery(string email, string password)
        {
            Email = email;
            Password = password;
        }

        // Parameterless constructor for model binding
        public LoginUserQuery()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}