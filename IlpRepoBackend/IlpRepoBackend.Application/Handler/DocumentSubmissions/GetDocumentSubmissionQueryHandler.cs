using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.DocumentSubmissions
{
    public class GetDocumentSubmissionQueryHandler : IRequestHandler<GetDocumentSubmissionQuery, ApiResponse<DocumentSubmissionDto>>
    {
        private readonly IDocumentSubmissionRepository _documentSubmissionRepository;

        public GetDocumentSubmissionQueryHandler(IDocumentSubmissionRepository documentSubmissionRepository)
        {
            _documentSubmissionRepository = documentSubmissionRepository;
        }

        public async Task<ApiResponse<DocumentSubmissionDto>> Handle(GetDocumentSubmissionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var submission = await _documentSubmissionRepository.GetSubmissionWithDetailsAsync(request.SubmissionId);

                if (submission == null)
                {
                    throw new NotFoundException("Document Submission", request.SubmissionId);
                }

                var submissionDto = new DocumentSubmissionDto
                {
                    Id = submission.Id,
                    SubmissionLink = submission.SubmissionLink,
                    FileName = submission.FileName,
                    FileType = submission.FileType,
                    DocumentName = submission.Document?.Name ?? "Unknown",
                    ProjectId = submission.ProjectId,
                    ProjectName = submission.Project?.ProjectName,
                    TraineeId = submission.TraineeId,
                    TraineeName = submission.Trainee?.User?.Username,
                    SubmissionDate = submission.SubmissionDate
                };

                return new ApiResponse<DocumentSubmissionDto>(submissionDto, "Document submission retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<DocumentSubmissionDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentSubmissionDto>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}