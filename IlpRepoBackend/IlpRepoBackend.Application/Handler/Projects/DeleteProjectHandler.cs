using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, ApiResponse<bool>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMentorForPRojectRepository _mentorForPRojectRepository;
        private readonly IPocForAProjectRepository _pocForAProjectRepository;
        private readonly IProjecTeamRepository _projecTeamRepository;

        public DeleteProjectHandler(
            IProjectRepository projectRepository,
            IMentorForPRojectRepository mentorForPRojectRepository,
            IPocForAProjectRepository pocForAProjectRepository,
            IProjecTeamRepository projecTeamRepository)
        {
            _projectRepository = projectRepository;
            _mentorForPRojectRepository = mentorForPRojectRepository;
            _pocForAProjectRepository = pocForAProjectRepository;
            _projecTeamRepository = projecTeamRepository;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            // 1️⃣ Check if project exists
            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                return ApiResponse<bool>.Fail($"Project with ID {request.ProjectId} not found");

            try
            {
                // 2️⃣ Delete all related ProjectTeam entries
                await _projecTeamRepository.DeleteAllByProjectIdAsync(request.ProjectId);

                // 3️⃣ Delete all related MenterForAProject entries
                await _mentorForPRojectRepository.DeleteAllByProjectIdAsync(request.ProjectId);

                // 4️⃣ Delete all related PocsForProject entries
                await _pocForAProjectRepository.DeleteAllByProjectIdAsync(request.ProjectId);

                // 5️⃣ Delete the project itself
                await _projectRepository.DeleteAsync(request.ProjectId);

                return ApiResponse<bool>.Success(true, "Project deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to delete project: {ex.Message}");
            }
        }
    }
}
