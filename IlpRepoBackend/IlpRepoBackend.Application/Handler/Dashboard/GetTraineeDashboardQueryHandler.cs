using IlpRepoBackend.Application.Dto.Dashboard;
using IlpRepoBackend.Application.Query.Dashboard;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Dashboard
{
    public class GetTraineeDashboardQueryHandler : IRequestHandler<GetTraineeDashboardQuery, ApiResponse<TraineeDashboardDto>>
    {
        private readonly ITraineeRepository _traineeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentRequestRepository _documentRequestRepository;

        public GetTraineeDashboardQueryHandler(
            ITraineeRepository traineeRepository,
            IProjectRepository projectRepository,
            ICurriculumRepository curriculumRepository,
            IDocumentRepository documentRepository,
            IDocumentRequestRepository documentRequestRepository)
        {
            _traineeRepository = traineeRepository;
            _projectRepository = projectRepository;
            _curriculumRepository = curriculumRepository;
            _documentRepository = documentRepository;
            _documentRequestRepository = documentRequestRepository;
        }

        public async Task<ApiResponse<TraineeDashboardDto>> Handle(GetTraineeDashboardQuery request, CancellationToken cancellationToken)
        {
            var trainee = await _traineeRepository.GetByUserIdWithDetailsAsync(request.UserId);
            if (trainee == null) return ApiResponse<TraineeDashboardDto>.Fail("Trainee not found.");

            // 1. Get Project Data
            var projectTeam = trainee.ProjectTeams.FirstOrDefault();
            ProjectDto? projectDto = null;
            if (projectTeam != null)
            {
                // why isn't it calculated correctly in db?
                var project = await _projectRepository.GetProjectWithDetailsAsync(projectTeam.ProjectId);
                var totalRequests = project.DocumentRequests.Count;
                var submittedRequestsCount = project.DocumentRequests.Count(req => req.DocumentSubmissions != null && req.DocumentSubmissions.Any());
                var progressPercentage = (int)Math.Round((double)submittedRequestsCount / totalRequests * 100);
                if (project != null)
                {
                    projectDto = new ProjectDto
                    {
                        Id = project.Id,
                        Title = project.ProjectName,
                        Status = project.Status,
                        Progress = progressPercentage,
                        Technologies = !string.IsNullOrEmpty(project.Technology) ? project.Technology.Split(',').Select(t => t.Trim()).ToList() : new(),
                        Team = new TeamDto
                        {
                            Number = project.ProjectTeams.Count,
                            Members = project.ProjectTeams.Select(pt => pt.Trainee.User.Username).ToList()
                        }
                    };
                }
            }

            // 2. Get Batch Data
            var batch = trainee.Batch;
            var today = DateTime.UtcNow.Date;
            var startDate = batch.StartDate?.Date ?? today;
            var endDate = batch.EndDate?.Date ?? today;

            var batchStatus = "Ongoing"; // Default assumption
            if (today < startDate)
            {
                batchStatus = "Not Started";
            }
            else if (today > endDate)
            {
                batchStatus = "Completed";
            }

            var batchDay = 0;
            if (batch.StartDate.HasValue)
            {
                switch (batchStatus)
                {
                    case "Ongoing":
                        // For ongoing batches, calculate the current day number (Day 1, Day 2, etc.)
                        batchDay = (today - startDate).Days + 1;
                        break;
                    case "Completed":
                        // For completed batches, calculate the total duration of the batch
                        if (batch.EndDate.HasValue)
                        {
                            batchDay = (endDate - startDate).Days + 1;
                        }
                        break;
                        // For "Not Started", batchDay remains 0 by default
                }
            }

            var batchDto = new BatchDto
            {
                Id = batch.Id,
                Title = batch.BatchName,
                Type = batch.BatchType?.Name ?? "N/A",
                StartDate = batch.StartDate ?? DateTime.MinValue,
                EndDate = batch.EndDate ?? DateTime.MinValue,
                Day = batchDay < 0 ? 0 : batchDay,
                Status = batchStatus
            };

            // 3. Get Scores and Rank
            var allTraineesInBatch = await _traineeRepository.GetTraineesWithResultsByBatchIdAsync(trainee.BatchId);
            var scores = CalculateScoresAndRank(trainee.Id, allTraineesInBatch);

            // 4. Get Sessions Data
            var sessions = (await _curriculumRepository.GetByBatchIdAsync(trainee.BatchId))
                .Select(s => new SessionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Category = s.Color ?? "default",
                    Date = s.Start
                }).ToList();

            // 5. Get Project-specific Documents
            var requiredDocuments = new List<DocumentDto>();
            if (projectDto != null)
            {
                var docRequests = await _documentRequestRepository.GetByProjectIdAsync(projectDto.Id);
                requiredDocuments = docRequests.Where(dr => dr.Document != null).Select(dr => new DocumentDto
                {
                    Id = dr.Document!.Id,
                    Title = dr.Document.Name,
                    Type = dr.Document.FileType,
                    UploadDate = dr.Document.UploadDate,
                    Url = dr.Document.Link
                }).ToList();
            }

            // 6. Assemble Dashboard DTO
            var dashboardDto = new TraineeDashboardDto
            {
                Profile = new ProfileDto
                {
                    FirstName = trainee.User.Username.Split(' ')[0],
                    BatchId = trainee.BatchId,
                    ProjectId = projectDto?.Id
                },
                Project = projectDto,
                Batch = batchDto,
                Scores = scores,
                Sessions = sessions,
                Documents = requiredDocuments
            };

            return ApiResponse<TraineeDashboardDto>.Success(dashboardDto);
        }

        private ScoresDto CalculateScoresAndRank(int currentTraineeId, List<Trainee> allTraineesInBatch)
        {
            if (!allTraineesInBatch.Any()) return new ScoresDto();

            var traineeScores = allTraineesInBatch
                .Select(t => new
                {
                    TraineeId = t.Id,
                    Average = t.Results.Any() ? t.Results.Average(r => r.ObtainedMark) : 0,
                    CourseScores = t.Results
                                    .GroupBy(r => r.Assessment?.Type.ToString() ?? "Unknown")
                                    .Select(g => new CourseScoreDto { Caption = g.Key, Value = g.Average(r => r.ObtainedMark) })
                                    .ToList()
                })
                .OrderByDescending(s => s.Average)
                .ToList();

            var currentUserScore = traineeScores.FirstOrDefault(s => s.TraineeId == currentTraineeId);
            if (currentUserScore == null) return new ScoresDto();

            var rank = traineeScores.FindIndex(s => s.TraineeId == currentTraineeId) + 1;

            return new ScoresDto
            {
                Average = currentUserScore.Average,
                Rank = rank,
                Courses = currentUserScore.CourseScores
            };
        }
    }
}
