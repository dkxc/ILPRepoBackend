using AutoMapper;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Query.Projects;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, ApiResponse<List<ProjectDto>>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetAllProjectsHandler(
            IProjectRepository projectRepository,
            IBatchRepository batchRepository,
            IMapper mapper)
        {
            _projectRepository = projectRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all projects with full details
                var projects = await _projectRepository.GetAllProjectsWithDetailsAsync();

                if (projects == null || !projects.Any())
                {
                    return ApiResponse<List<ProjectDto>>.Success(
                        new List<ProjectDto>(),
                        "No projects found");
                }

                // Apply filters if provided
                var filteredProjects = projects.AsQueryable();

                // Filter by status if provided
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (Enum.TryParse<ProjectStatus>(request.Status, true, out var statusEnum))
                    {
                        filteredProjects = filteredProjects.Where(p => p.Status == statusEnum);
                    }
                }

                // Filter by technology if provided
                if (!string.IsNullOrWhiteSpace(request.Technology))
                {
                    filteredProjects = filteredProjects.Where(p =>
                        p.Technology != null &&
                        p.Technology.Contains(request.Technology, StringComparison.OrdinalIgnoreCase));
                }

                var projectsList = filteredProjects.ToList();

                // Map to DTOs
                var projectDtos = new List<ProjectDto>();

                foreach (var project in projectsList)
                {
                    var projectDto = _mapper.Map<ProjectDto>(project);

                    // Get batch information if needed
                    // Assuming you have a way to determine batch from project or project teams
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

                    projectDtos.Add(projectDto);
                }

                return ApiResponse<List<ProjectDto>>.Success(
                    projectDtos,
                    $"Retrieved {projectDtos.Count} projects successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ProjectDto>>.Fail($"Error retrieving projects: {ex.Message}");
            }
        }
    }
}