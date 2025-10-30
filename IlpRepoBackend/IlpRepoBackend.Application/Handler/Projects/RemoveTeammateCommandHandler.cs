using IlpRepoBackend.Application.Command;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class RemoveTeammateCommandHandler : IRequestHandler<RemoveTeammateCommand, ApiResponse<bool>>
    {
        private readonly IProjectRepository _projectRepository;

        public RemoveTeammateCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<ApiResponse<bool>> Handle(RemoveTeammateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if project exists
                var projectExists = await _projectRepository.ExistsAsync(request.ProjectId);
                if (!projectExists)
                {
                    throw new NotFoundException("Project", request.ProjectId);
                }

                // Check if the trainee is actually part of the project
                var isTraineeInProject = await _projectRepository.IsTraineeInProjectAsync(request.ProjectId, request.TraineeId);
                if (!isTraineeInProject)
                {
                    return new ApiResponse<bool>("Trainee is not a member of this project", 400);
                }

                // Get the project team relationship
                var projectTeam = await _projectRepository.GetProjectTeammateAsync(request.ProjectId, request.TraineeId);
                if (projectTeam == null)
                {
                    return new ApiResponse<bool>("Project team relationship not found", 404);
                }

                // Remove the teammate from the project
                await _projectRepository.RemoveTeammateAsync(projectTeam);

                return new ApiResponse<bool>(true, $"Teammate '{projectTeam.Trainee.User.Username}' successfully removed from the project");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<bool>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>($"An error occurred while removing teammate: {ex.Message}", 500);
            }
        }
    }
}