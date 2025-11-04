using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Enum;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.AdminDashboard
{
    public class GetProjectsByBatchIdQueryHandler : IRequestHandler<GetProjectsByBatchIdQuery, List<ProjectsByBatchIdDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IProjecTeamRepository _projectTeamRepository;
        private readonly ITraineeRepository _traineeRepository;

        public GetProjectsByBatchIdQueryHandler(
            IProjectRepository projectRepository,
            IProjecTeamRepository projectTeamRepository,
            ITraineeRepository traineeRepository)
        {
            _projectRepository = projectRepository;
            _projectTeamRepository = projectTeamRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<List<ProjectsByBatchIdDto>> Handle(GetProjectsByBatchIdQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Get all trainees in this batch
            var trainees = await _traineeRepository.GetAllAsync();
            var traineesInBatch = trainees.Where(t => t.BatchId == request.BatchId).ToList();

            // 2️⃣ Get all project_team entries for these trainees
            var allTeams = await _projectTeamRepository.GetAllAsync();
            var batchProjectTeams = allTeams
                .Where(pt => traineesInBatch.Any(t => t.Id == pt.TraineeId))
                .ToList();

            // 3️⃣ Get unique project IDs for this batch
            var projectIds = batchProjectTeams.Select(pt => pt.ProjectId).Distinct().ToList();

            // 4️⃣ Get projects belonging to these IDs
            var allProjects = await _projectRepository.GetAllAsync();
            var projectsInBatch = allProjects
                .Where(p => projectIds.Contains(p.Id))
                .ToList();

            // 5️⃣ Build DTO
            var result = new List<ProjectsByBatchIdDto>();

            foreach (var project in projectsInBatch)
            {
                var teamMembers = batchProjectTeams
                    .Where(pt => pt.ProjectId == project.Id)
                    .ToList();

                // ✅ Type-safe enum comparison (no string comparison)
                var teamLead = teamMembers.FirstOrDefault(pt => pt.Role == ProjectRole.TeamLead);

                var leadName = string.Empty;
                if (teamLead != null)
                {
                    var leadTrainee = traineesInBatch.FirstOrDefault(t => t.Id == teamLead.TraineeId);
                    if (leadTrainee != null)
                        leadName = leadTrainee.Email ?? "N/A"; // You can replace this with trainee name if you have one
                }

                result.Add(new ProjectsByBatchIdDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.ProjectName,
                    TeamLeadName = leadName,
                    NoOfTrainees = teamMembers.Count,
                    Technology = project.Technology ?? "N/A",
                    SubmissionRate = 0, // Placeholder until API ready
                    Status = project.Status.ToString()
                });
            }



            return result;
        }
    }
}
