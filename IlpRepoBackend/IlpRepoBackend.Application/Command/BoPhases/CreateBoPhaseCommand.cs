using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.BoPhases
{
    public class CreateBoPhaseCommand : IRequest<ApiResponse<BoPhaseDetailsDto>>
    {
        public string TraineeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BuddyName { get; set; } = string.Empty;
        public string? DuName { get; set; }  // Added DU name
    }
}