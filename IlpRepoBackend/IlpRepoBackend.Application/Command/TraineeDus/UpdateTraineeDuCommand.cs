using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command.TraineeDus
{
    public class UpdateTraineeDuCommand : IRequest<ApiResponse<TraineeDuDetailsDto>>
    {
        public int TraineeDuId { get; set; }
        public string? TraineeName { get; set; }
        public string DuAllocated { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? OjtMentor { get; set; }
    }
}