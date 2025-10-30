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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, ApiResponse<ProjectDto>>
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

        public CreateProjectHandler(
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

        public async Task<ApiResponse<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ProjectData;

            // 1️⃣ Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
            if (batch == null)
                return ApiResponse<ProjectDto>.Fail("Batch does not exist");

            // 2️⃣ Fetch trainees in the batch
            var batchTrainees = (await _traineeRepository.GetByBatchIdAsync(dto.BatchId)).ToList();

            // --- Validate Team Lead ---
            Trainee? teamLead = null;
            if (!string.IsNullOrWhiteSpace(dto.TeamLeadName))
            {
                var leadUser = await _userRepository.GetByUsernameAsync(dto.TeamLeadName.Trim());
                if (leadUser == null)
                    return ApiResponse<ProjectDto>.Fail($"Team Lead '{dto.TeamLeadName}' not found as a user");

                teamLead = batchTrainees.FirstOrDefault(t => t.UserId == leadUser.Id);
                if (teamLead == null)
                    return ApiResponse<ProjectDto>.Fail($"Team Lead '{dto.TeamLeadName}' not found in batch {dto.BatchId}");
            }

            // --- Validate Scrum Master ---
            Trainee? scrumMaster = null;
            if (!string.IsNullOrWhiteSpace(dto.ScrumMasterName))
            {
                var scrumUser = await _userRepository.GetByUsernameAsync(dto.ScrumMasterName.Trim());
                if (scrumUser == null)
                    return ApiResponse<ProjectDto>.Fail($"Scrum Master '{dto.ScrumMasterName}' not found as a user");

                scrumMaster = batchTrainees.FirstOrDefault(t => t.UserId == scrumUser.Id);
                if (scrumMaster == null)
                    return ApiResponse<ProjectDto>.Fail($"Scrum Master '{dto.ScrumMasterName}' not found in batch {dto.BatchId}");
            }

            // 3️⃣ Validate team members and cache them
            var teamMemberTrainees = new List<Trainee>();
            if (dto.TeamMembers != null && dto.TeamMembers.Any())
            {
                foreach (var name in dto.TeamMembers)
                {
                    Trainee? member = await _traineeRepository.GetByName(name);
                    if (member == null)
                        return ApiResponse<ProjectDto>.Fail($"Trainee '{name}' not found");

                    var trainee = batchTrainees.FirstOrDefault(t => t.Id == member.Id);
                    if (trainee == null)
                        return ApiResponse<ProjectDto>.Fail($"Trainee '{name}' does not belong to batch {dto.BatchId}");

                    teamMemberTrainees.Add(member);
                }
            }

            // 4️⃣ Handle mentors (email may be null)
            var mentorIds = new List<int>();
            if (dto.Mentors != null && dto.Mentors.Any())
            {
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
            }

            // 5️⃣ Handle POCs (email may be null)
            var pocIds = new List<int>();
            if (dto.Pocs != null && dto.Pocs.Any())
            {
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
            }

            // 6️⃣ Create the project
            var project = new Project
            {
                ProjectName = dto.ProjectName,
                Technology = dto.Technology,
                Status = dto.Status,
                Progress = dto.Progress,
                //BatchId = dto.BatchId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.AddAsync(project);
            if (createdProject == null)
                return ApiResponse<ProjectDto>.Fail("Failed to create project");

            // 7️⃣ Add team members (use cached list)
            foreach (var member in teamMemberTrainees)
            {
                await _projecTeamRepository.AddAsync(new ProjectTeam
                {
                    ProjectId = createdProject.Id,
                    TraineeId = member.Id,
                    Role = ProjectRole.Trainee,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Add Team Lead
            if (teamLead != null)
            {
                await _projecTeamRepository.AddAsync(new ProjectTeam
                {
                    ProjectId = createdProject.Id,
                    TraineeId = teamLead.Id,
                    Role = ProjectRole.TeamLead,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Add Scrum Master
            if (scrumMaster != null)
            {
                await _projecTeamRepository.AddAsync(new ProjectTeam
                {
                    ProjectId = createdProject.Id,
                    TraineeId = scrumMaster.Id,
                    Role = ProjectRole.ScrumMaster,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 8️⃣ Assign mentors
            foreach (var mentorId in mentorIds.Distinct())
            {
                var m = await _mentorRepository.GetByIdAsync(mentorId);
                if (m != null)
                {
                    await _mentorForPRojectRepository.AddAsync(new MenterForAProject
                    {
                        ProjectId = createdProject.Id,
                        MenterId = mentorId,
                        MentorType = m.MentorType
                    });
                }
            }

            // 9️⃣ Assign POCs
            foreach (var pocId in pocIds.Distinct())
            {
                await _pocForAProjectRepository.AddAsync(new PocsForProject
                {
                    ProjectId = createdProject.Id,
                    PocId = pocId
                });
            }

            // 🔟 Return full project
            // 🔟 Return full project with all related data
            var fullProject = await _projectRepository.GetProjectWithDetailsAsync(createdProject.Id);
            if (fullProject == null)
                return ApiResponse<ProjectDto>.Fail("Failed to retrieve created project");

            var projectDto = _mapper.Map<ProjectDto>(fullProject);
            return ApiResponse<ProjectDto>.Success(projectDto);
        }
    }
}