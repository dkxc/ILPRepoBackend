using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.TraineeDus
{
    public class CreateTraineeDuCommand : IRequest<ApiResponse<TraineeDuDto>>
    {
        public string TraineeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DuName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string OjtMenter { get; set; } = string.Empty;
    }
}