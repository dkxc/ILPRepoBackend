using IlpRepoBackend.Application.Wrapper;
using MediatR;

namespace IlpRepoBackend.Application.Command
{
    public class DeleteCurriculumCommand : IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
        public int BatchId { get; set; } // Included for validation to ensure the event belongs to the batch
    }
}
