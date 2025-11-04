using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class UpdateCurriculumCommand : IRequest<ApiResponse<CurriculumDto>>
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public UpdateCurriculumDto UpdateCurriculumDto { get; set; }
    }
}
