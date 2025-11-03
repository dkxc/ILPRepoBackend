using AutoMapper;
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
    public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ApiResponse<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetProjectByIdHandler(
            IProjectRepository projectRepository,
            IBatchRepository batchRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetProjectWithDetailsAsync(request.ProjectId);

                if (project == null)
                {
                    return ApiResponse<ProjectDto>.Fail($"Project with ID {request.ProjectId} not found");
                }

                var projectDto = _mapper.Map<ProjectDto>(project);

                // Get batch information from the project's team members
                var firstTeam = project.ProjectTeams?.FirstOrDefault();
                if (firstTeam?.Trainee?.BatchId != null)
                {
                    var batch = await _batchRepository.GetByIdAsync(firstTeam.Trainee.BatchId);
                    if (batch != null)
                    {
                        projectDto.BatchId = batch.Id;
                        projectDto.BatchName = batch.BatchName ?? string.Empty;
                    }
                }

                return ApiResponse<ProjectDto>.Success(projectDto, "Project retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ProjectDto>.Fail($"Error retrieving project: {ex.Message}");
            }
        }
    }
}