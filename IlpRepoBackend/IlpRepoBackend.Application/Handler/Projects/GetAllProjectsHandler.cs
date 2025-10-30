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
        private readonly IMapper _mapper;

        public GetAllProjectsHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<ProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get all projects with full details - FIXED: Use method that loads related entities
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

                // Map to DTOs
                var projectDtos = _mapper.Map<List<ProjectDto>>(filteredProjects.ToList());

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