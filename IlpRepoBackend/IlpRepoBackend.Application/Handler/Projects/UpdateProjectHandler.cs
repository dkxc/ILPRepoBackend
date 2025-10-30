using AutoMapper;
using IlpRepoBackend.Application.Command.Projects;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
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
    public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, ApiResponse<ProjectDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMentorRepository _mentorRepository;
        private readonly IMentorForPRojectRepository _mentorForPRojectRepository;
        private readonly IPocForAProjectRepository _pocForAProjectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPocRepository _pocRepository;
        private readonly IProjecTeamRepository _projecTeamRepository;

        public UpdateProjectHandler(
            IProjectRepository projectRepository,
            IBatchRepository batchRepository,
            ITraineeRepository traineeRepository,
            IMentorRepository mentorRepository,
            IPocForAProjectRepository pocForAProjectRepository,
            IPocRepository pocRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IMentorForPRojectRepository mentorForPRojectRepository,
            IProjecTeamRepository projecTeamRepository)
        {
            _projectRepository = projectRepository;
            _batchRepository = batchRepository;
            _traineeRepository = traineeRepository;
            _mentorRepository = mentorRepository;
            _pocForAProjectRepository = pocForAProjectRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _pocRepository = pocRepository;
            _mentorForPRojectRepository = mentorForPRojectRepository;
            _projecTeamRepository = projecTeamRepository;
        }

        public async Task<ApiResponse<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProjectData;

            // 1️⃣ Check if project exists
            var existingProject = await _projectRepository.GetByIdAsync(request.ProjectId);
            if (existingProject == null)
                return ApiResponse<ProjectDto>.Fail($"Project with ID {request.ProjectId} not found");

            // 2️⃣ Validate batch exists if BatchId is provided
            if (dto.BatchId > 0)
            {
                var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
                if (batch == null)
                    return ApiResponse<ProjectDto>.Fail("Batch does not exist");
            }

            // 3️⃣ Update basic project properties
            existingProject.ProjectName = dto.ProjectName ?? existingProject.ProjectName;
            existingProject.Technology = dto.Technology ?? existingProject.Technology;
            existingProject.Status = dto.Status;
            existingProject.Progress = dto.Progress;
            existingProject.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(existingProject);

            // 4️⃣ Update Team Members if provided
            if (dto.TeamMembers != null)
            {
                // Fetch trainees in the batch
                var batchTrainees = (await _traineeRepository.GetByBatchIdAsync(dto.BatchId)).ToList();

                // Remove existing team members (Trainee role only)
                var existingTeamMembers = await _projecTeamRepository.GetByProjectIdAsync(request.ProjectId);
                var traineesToRemove = existingTeamMembers.Where(pt => pt.Role == ProjectRole.Trainee).ToList();

                foreach (var member in traineesToRemove)
                {
                    await _projecTeamRepository.DeleteAsync(member.Id);
                }

                // Add new team members
                foreach (var name in dto.TeamMembers)
                {
                    var member = await _traineeRepository.GetByName(name);
                    if (member == null)
                        return ApiResponse<ProjectDto>.Fail($"Trainee '{name}' not found");

                    var trainee = batchTrainees.FirstOrDefault(t => t.Id == member.Id);
                    if (trainee == null)
                        return ApiResponse<ProjectDto>.Fail($"Trainee '{name}' does not belong to batch {dto.BatchId}");

                    await _projecTeamRepository.AddAsync(new ProjectTeam
                    {
                        ProjectId = request.ProjectId,
                        TraineeId = member.Id,
                        Role = ProjectRole.Trainee,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // 5️⃣ Update Team Lead if provided
            if (!string.IsNullOrWhiteSpace(dto.TeamLeadName))
            {
                var batchTrainees = (await _traineeRepository.GetByBatchIdAsync(dto.BatchId)).ToList();
                var leadUser = await _userRepository.GetByUsernameAsync(dto.TeamLeadName.Trim());

                if (leadUser == null)
                    return ApiResponse<ProjectDto>.Fail($"Team Lead '{dto.TeamLeadName}' not found as a user");

                var teamLead = batchTrainees.FirstOrDefault(t => t.UserId == leadUser.Id);
                if (teamLead == null)
                    return ApiResponse<ProjectDto>.Fail($"Team Lead '{dto.TeamLeadName}' not found in batch {dto.BatchId}");

                // Remove existing team lead
                var existingTeamMembers = await _projecTeamRepository.GetByProjectIdAsync(request.ProjectId);
                var existingLead = existingTeamMembers.FirstOrDefault(pt => pt.Role == ProjectRole.TeamLead);
                if (existingLead != null)
                {
                    await _projecTeamRepository.DeleteAsync(existingLead.Id);
                }

                // Add new team lead
                await _projecTeamRepository.AddAsync(new ProjectTeam
                {
                    ProjectId = request.ProjectId,
                    TraineeId = teamLead.Id,
                    Role = ProjectRole.TeamLead,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 6️⃣ Update Scrum Master if provided
            if (!string.IsNullOrWhiteSpace(dto.ScrumMasterName))
            {
                var batchTrainees = (await _traineeRepository.GetByBatchIdAsync(dto.BatchId)).ToList();
                var scrumUser = await _userRepository.GetByUsernameAsync(dto.ScrumMasterName.Trim());

                if (scrumUser == null)
                    return ApiResponse<ProjectDto>.Fail($"Scrum Master '{dto.ScrumMasterName}' not found as a user");

                var scrumMaster = batchTrainees.FirstOrDefault(t => t.UserId == scrumUser.Id);
                if (scrumMaster == null)
                    return ApiResponse<ProjectDto>.Fail($"Scrum Master '{dto.ScrumMasterName}' not found in batch {dto.BatchId}");

                // Remove existing scrum master
                var existingTeamMembers = await _projecTeamRepository.GetByProjectIdAsync(request.ProjectId);
                var existingScrumMaster = existingTeamMembers.FirstOrDefault(pt => pt.Role == ProjectRole.ScrumMaster);
                if (existingScrumMaster != null)
                {
                    await _projecTeamRepository.DeleteAsync(existingScrumMaster.Id);
                }

                // Add new scrum master
                await _projecTeamRepository.AddAsync(new ProjectTeam
                {
                    ProjectId = request.ProjectId,
                    TraineeId = scrumMaster.Id,
                    Role = ProjectRole.ScrumMaster,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 7️⃣ Update Mentors if provided
            if (dto.Mentors != null)
            {
                // Remove existing mentors
                var existingMentors = await _mentorForPRojectRepository.GetByProjectIdAsync(request.ProjectId);
                foreach (var mentor in existingMentors)
                {
                    await _mentorForPRojectRepository.DeleteAsync(mentor.Id);
                }

                // Add new mentors
                var mentorIds = new List<int>();
                foreach (var mentor in dto.Mentors)
                {
                    Mentor? existing = null;

                    if (!string.IsNullOrWhiteSpace(mentor.Email))
                        existing = await _mentorRepository.GetByEmailAsync(mentor.Email!);
                    else if (!string.IsNullOrWhiteSpace(mentor.Name))
                        existing = await _mentorRepository.GetByNameAsync(mentor.Name.Trim());

                    if (existing == null)
                    {
                        var newMentor = new Mentor
                        {
                            Name = mentor.Name,
                            Email = mentor.Email
                        };

                        var createdMentor = await _mentorRepository.AddAsync(newMentor);
                        mentorIds.Add(createdMentor.Id);
                    }
                    else
                    {
                        mentorIds.Add(existing.Id);
                    }
                }

                // Assign mentors
                foreach (var mentorId in mentorIds.Distinct())
                {
                    var m = await _mentorRepository.GetByIdAsync(mentorId);
                    if (m != null)
                    {
                        await _mentorForPRojectRepository.AddAsync(new MenterForAProject
                        {
                            ProjectId = request.ProjectId,
                            MenterId = mentorId,
                            MentorType = m.MentorType
                        });
                    }
                }
            }

            // 8️⃣ Update POCs if provided
            if (dto.Pocs != null)
            {
                // Remove existing POCs
                var existingPocs = await _pocForAProjectRepository.GetByProjectIdAsync(request.ProjectId);
                foreach (var poc in existingPocs)
                {
                    await _pocForAProjectRepository.DeleteAsync(poc.Id);
                }

                // Add new POCs
                var pocIds = new List<int>();
                foreach (var poc in dto.Pocs)
                {
                    Poc? existing = null;

                    if (!string.IsNullOrWhiteSpace(poc.Email))
                        existing = await _pocRepository.GetByEmailAsync(poc.Email!);
                    else if (!string.IsNullOrWhiteSpace(poc.Name))
                        existing = await _pocRepository.GetByNameAsync(poc.Name.Trim());

                    if (existing == null)
                    {
                        var newPoc = new Poc
                        {
                            Name = poc.Name,
                            Email = poc.Email,
                        };

                        var createdPoc = await _pocRepository.AddAsync(newPoc);
                        pocIds.Add(createdPoc.Id);
                    }
                    else
                    {
                        pocIds.Add(existing.Id);
                    }
                }

                // Assign POCs
                foreach (var pocId in pocIds.Distinct())
                {
                    await _pocForAProjectRepository.AddAsync(new PocsForProject
                    {
                        ProjectId = request.ProjectId,
                        PocId = pocId
                    });
                }
            }

            // 9️⃣ Return updated project with all details
            var fullProject = await _projectRepository.GetProjectWithDetailsAsync(request.ProjectId);
            if (fullProject == null)
                return ApiResponse<ProjectDto>.Fail("Failed to retrieve updated project");

            var projectDto = _mapper.Map<ProjectDto>(fullProject);
            return ApiResponse<ProjectDto>.Success(projectDto);
        }
    }

    // ==================== DELETE HANDLER ====================
    
}