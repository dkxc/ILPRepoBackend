using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class GetProjectDetailsByIdHandler : IRequestHandler<GetProjectDetailsByIdQuery, ApiResponse<ProjectDetailsDto>>
    {
        private readonly IProjectRepository _projectRepository;

        public GetProjectDetailsByIdHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ApiResponse<ProjectDetailsDto>> Handle(GetProjectDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetProjectWithDetailsAsync(request.ProjectId);

                if (project == null)
                {
                    return ApiResponse<ProjectDetailsDto>.Fail($"Project with ID {request.ProjectId} not found");
                }

                // Map to ProjectDetailsDto
                var projectDetails = new ProjectDetailsDto
                {
                    Id = project.Id,
                    ProjectName = project.ProjectName,
                    Status = project.Status,
                    Progress = project.Progress,
                    TechnologyStack = project.Technology ?? string.Empty, // For display and textbox population
                    Trainees = project.ProjectTeams?
                        .Where(pt => pt.Trainee != null && pt.Trainee.User != null)
                        .Select(pt => new TraineeInfoDto
                        {
                            Name = pt.Trainee.User.Username ?? string.Empty,
                            Email = pt.Trainee.User.Email
                        })
                        .ToList() ?? new(),
                    ProjectLinks = project.ProjectLinks?
                        .Select(pl => new ProjectLinkDetailDto
                        {
                            Id = pl.Id, // ProjectLink ID for editing/deletion
                            LinkId = pl.LinkId, // Link type ID for dropdown population
                            LinkUrl = pl.LinkUrl, // URL for display and textbox population
                            LinkTypeName = pl.Link?.Name // Link type name for display
                        })
                        .ToList() ?? new(),
                    DocumentSubmissions = project.DocumentRequests?
                        .SelectMany(dr => dr.DocumentSubmissions ?? new List<Domain.Entities.DocumentSubmission>())
                        .Select(ds => new DocumentSubmissionDto
                        {
                            Id = ds.Id,
                            FileName = ds.FileName,
                            FileType = ds.FileType,
                            SubmissionLink = ds.SubmissionLink,
                            SubmissionDate = ds.SubmissionDate,
                            DocumentName = ds.Document?.Name, // Name from Document entity
                            RequestDueDate = ds.DocumentRequest?.DueDate // Due date from DocumentRequest
                        })
                        .ToList() ?? new()
                };

                return ApiResponse<ProjectDetailsDto>.Success(projectDetails, "Project details retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectDetailsDto>.Fail($"Error retrieving project details: {ex.Message}");
            }
        }
    }
}
