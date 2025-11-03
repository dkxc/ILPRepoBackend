using IlpRepoBackend.Application.Query.Auth;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;


namespace IlpRepoBackend.Application.Handler.Auth
{
    public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, bool>
    {
        private readonly IAuthService _authService;

        public ValidateTokenQueryHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<bool> Handle(ValidateTokenQuery request, CancellationToken cancellationToken)
        {
            var result = _authService.ValidateToken(request.Token);
            return Task.FromResult(result);
        }
    }

}
