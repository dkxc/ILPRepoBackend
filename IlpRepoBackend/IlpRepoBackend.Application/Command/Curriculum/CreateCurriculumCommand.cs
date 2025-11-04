using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class CreateCurriculumCommand : IRequest<ApiResponse<CurriculumDto>>
    {
        public int BatchId { get; set; }
        public CreateCurriculumDto CreateCurriculumDto { get; set; }
    }
}
