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
    public class CreateBatchProjectsHandler : IRequestHandler<CreateBatchProjectsCommand, ApiResponse<List<ProjectDto>>>
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

        public CreateBatchProjectsHandler(
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

        public async Task<ApiResponse<List<ProjectDto>>> Handle(CreateBatchProjectsCommand request, CancellationToken cancellationToken)
        {
            var dto = request.BatchProjectsData;

            // 1️⃣ Validate batch exists
            var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
            if (batch == null)
                return ApiResponse<List<ProjectDto>>.Fail("Batch does not exist");

            if (dto.Projects == null || !dto.Projects.Any())
                return ApiResponse<List<ProjectDto>>.Fail("No projects provided");

            // 2️⃣ Fetch all trainees in the batch once (optimization)
            var batchTrainees = (await _traineeRepository.GetByBatchIdAsync(dto.BatchId)).ToList();

            var createdProjectDtos = new List<ProjectDto>();
            var errors = new List<string>();

            // 3️⃣ Process each project
            for (int i = 0; i < dto.Projects.Count; i++)
            {
                var projectInfo = dto.Projects[i];
                var projectNumber = i + 1;

                try
                {
                    var projectDto = await CreateSingleProject(projectInfo, dto.BatchId, batch.BatchName, batchTrainees, projectNumber);

                    if (projectDto != null)
                    {
                        createdProjectDtos.Add(projectDto);
                    }
                    else
                    {
                        errors.Add($"Project {projectNumber} ({projectInfo.ProjectName}): Failed to create");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Project {projectNumber} ({projectInfo.ProjectName}): {ex.Message}");
                }
            }

            // 4️⃣ Validate all projects were created
            int expectedCount = dto.Projects.Count;
            int actualCount = createdProjectDtos.Count;

            if (actualCount == expectedCount && !errors.Any())
            {
                return ApiResponse<List<ProjectDto>>.Success(
                    createdProjectDtos,
                    $"Successfully created all {actualCount} projects");
            }
            else if (actualCount > 0 && errors.Any())
            {
                var message = $"Partially successful: Created {actualCount}/{expectedCount} projects. Errors: {string.Join("; ", errors)}";
                return ApiResponse<List<ProjectDto>>.Success(createdProjectDtos, message);
            }
            else
            {
                return ApiResponse<List<ProjectDto>>.Fail(
                    $"Failed to create any projects. Errors: {string.Join("; ", errors)}");
            }
        }

        private async Task<ProjectDto?> CreateSingleProject(
            ProjectCreateInfo projectInfo,
            int batchId,
            string batchName,
            List<Trainee> batchTrainees,
            int projectNumber)
        {
            // Validate Team Lead
            Trainee? teamLead = null;
            if (!string.IsNullOrWhiteSpace(projectInfo.TeamLeadName))
            {
                var leadUser = await _userRepository.GetByUsernameAsync(projectInfo.TeamLeadName.Trim());
                if (leadUser == null)
                    throw new InvalidOperationException($"Team Lead '{projectInfo.TeamLeadName}' not found");

                teamLead = batchTrainees.FirstOrDefault(t => t.UserId == leadUser.Id);
                if (teamLead == null)
                    throw new InvalidOperationException($"Team Lead '{projectInfo.TeamLeadName}' not in batch");
            }

            // Validate Scrum Master
            Trainee? scrumMaster = null;
            if (!string.IsNullOrWhiteSpace(projectInfo.ScrumMasterName))
            {
                var scrumUser = await _userRepository.GetByUsernameAsync(projectInfo.ScrumMasterName.Trim());
                if (scrumUser == null)
                    throw new InvalidOperationException($"Scrum Master '{projectInfo.ScrumMasterName}' not found");

                scrumMaster = batchTrainees.FirstOrDefault(t => t.UserId == scrumUser.Id);
                if (scrumMaster == null)
                    throw new InvalidOperationException($"Scrum Master '{projectInfo.ScrumMasterName}' not in batch");
            }

            // Validate team members
            var teamMemberTrainees = new List<Trainee>();
            if (projectInfo.TeamMembers != null && projectInfo.TeamMembers.Any())
            {
                foreach (var name in projectInfo.TeamMembers)
                {
                    Trainee? member = await _traineeRepository.GetByName(name);
                    if (member == null)
                        throw new InvalidOperationException($"Trainee '{name}' not found");

                    var trainee = batchTrainees.FirstOrDefault(t => t.Id == member.Id);
                    if (trainee == null)
                        throw new InvalidOperationException($"Trainee '{name}' not in batch");

                    teamMemberTrainees.Add(member);
                }
            }

            // Handle mentors
            var mentorIds = new List<int>();
            if (projectInfo.Mentors != null && projectInfo.Mentors.Any())
            {
                foreach (var mentor in projectInfo.Mentors)
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
                            Email = mentor.Email,
                            MentorType = mentor.MentorType
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

            // Handle POCs
            var pocIds = new List<int>();
            if (projectInfo.Pocs != null && projectInfo.Pocs.Any())
            {
                foreach (var poc in projectInfo.Pocs)
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

            // Create the project
            var project = new Project
            {
                ProjectName = projectInfo.ProjectName,
                Technology = projectInfo.Technology,
                Status = projectInfo.Status,
                Progress = projectInfo.Progress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.AddAsync(project);
            if (createdProject == null)
                throw new InvalidOperationException("Failed to create project in database");

            // Add team members
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

                // ✅ UPDATE USER ROLE TO TEAMLEAD
                if (teamLead.User != null && teamLead.User.Role == UserRole.Trainee)
                {
                    teamLead.User.Role = UserRole.TeamLead;
                    teamLead.User.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(teamLead.User);
                }
            }

            // Add Scrum Master (NO role change - stays Trainee)
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

            // Assign mentors
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

            // Assign POCs
            foreach (var pocId in pocIds.Distinct())
            {
                await _pocForAProjectRepository.AddAsync(new PocsForProject
                {
                    ProjectId = createdProject.Id,
                    PocId = pocId
                });
            }

            // ✅ Fetch mentors and POCs properly (avoid .Result antipattern)
            var mentorDtos = new List<MentorDto>();
            foreach (var mentorId in mentorIds.Distinct())
            {
                var m = await _mentorRepository.GetByIdAsync(mentorId);
                if (m != null)
                {
                    mentorDtos.Add(new MentorDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Email = m.Email,
                        MentorType = m.MentorType
                    });
                }
            }

            var pocDtos = new List<PocDto>();
            foreach (var pocId in pocIds.Distinct())
            {
                var p = await _pocRepository.GetByIdAsync(pocId);
                if (p != null)
                {
                    pocDtos.Add(new PocDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Email = p.Email
                    });
                }
            }

            // Create DTO with proper mapping
            var projectDto = new ProjectDto
            {
                Id = createdProject.Id,
                ProjectName = createdProject.ProjectName,
                Technology = createdProject.Technology,
                Status = createdProject.Status,
                Progress = createdProject.Progress,
                BatchId = batchId,
                BatchName = batchName ?? string.Empty,
                TeamMembers = teamMemberTrainees.Select(t => t.User?.Username ?? "Unknown").ToList(),
                TeamLead = teamLead?.User?.Username,
                ScrumMaster = scrumMaster?.User?.Username,
                Mentors = mentorDtos,
                Pocs = pocDtos,
                DocumentRequests = new List<DocumentRequestDto>(),
                DocumentSubmissions = new List<DocumentSubmissionDto>(),
                CreatedAt = createdProject.CreatedAt,
                UpdatedAt = createdProject.UpdatedAt
            };

            return projectDto;
        }
    }
}