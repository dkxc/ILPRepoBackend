using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class UpdateProjectTechnologyHandler : IRequestHandler<UpdateProjectTechnologyCommand, ApiResponse<bool>>
    {
        private readonly IProjectRepository _projectRepository;

        public UpdateProjectTechnologyHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateProjectTechnologyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(request.ProjectId);
                
                if (project == null)
                {
                    return ApiResponse<bool>.Fail($"Project with ID {request.ProjectId} not found");
                }

                project.Technology = request.Technology;
                project.UpdatedAt = DateTime.UtcNow;

                await _projectRepository.UpdateAsync(project);

                return ApiResponse<bool>.Success(true, "Technology stack updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Error updating technology stack: {ex.Message}");
            }
        }
    }
}