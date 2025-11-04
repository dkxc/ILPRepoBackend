using IlpRepoBackend.Application.Commands.Results;
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

namespace IlpRepoBackend.Application.Handlers.Results
{
    public class UploadResultsCommandHandler : IRequestHandler<UploadResultsCommand, ApiResponse<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IResultRepository _resultRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFeedbackHeaderRepository _feedbackHeaderRepository;
        private readonly IFeedbackHeaderResponseRepository _feedbackHeaderResponseRepository;

        public UploadResultsCommandHandler(
            IUserRepository userRepository,
            ITraineeRepository traineeRepository,
            IAssessmentRepository assessmentRepository,
            IResultRepository resultRepository,
            IFeedbackRepository feedbackRepository,
            IFeedbackHeaderRepository feedbackHeaderRepository,
            IFeedbackHeaderResponseRepository feedbackHeaderResponseRepository)
        {
            _userRepository = userRepository;
            _traineeRepository = traineeRepository;
            _assessmentRepository = assessmentRepository;
            _resultRepository = resultRepository;
            _feedbackRepository = feedbackRepository;
            _feedbackHeaderRepository = feedbackHeaderRepository;
            _feedbackHeaderResponseRepository = feedbackHeaderResponseRepository;
        }

        public async Task<ApiResponse<string>> Handle(UploadResultsCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();

            if (request.Scores.Any())
            {
                await ProcessScores(request.Scores, errors);
            }

            if (request.Feedbacks.Any())
            {
                await ProcessFeedbacks(request.Feedbacks, errors);
            }

            if (errors.Any())
            {
                return ApiResponse<string>.Fail($"Processing completed with errors: {string.Join(", ", errors)}");
            }

            return ApiResponse<string>.Success("Scores and feedback processed successfully.");
        }

        private async Task ProcessScores(List<TraineeScoreUploadDto> scores, List<string> errors)
        {
            var assessments = await GetAssessments();

            foreach (var scoreData in scores)
            {
                var trainee = await GetTraineeByEmail(scoreData.Email, errors);
                if (trainee == null) continue;

                await CreateOrUpdateResult(trainee.Id, assessments[AssessmentType.TechFundamentals].Id, scoreData.TechFundamentalsScore);
                await CreateOrUpdateResult(trainee.Id, assessments[AssessmentType.Specialisation].Id, scoreData.SpecializationScore);
                await CreateOrUpdateResult(trainee.Id, assessments[AssessmentType.BO].Id, scoreData.BusinessOrientationScore);
            }
        }

        private async Task ProcessFeedbacks(List<TraineeFeedbackUploadDto> feedbacks, List<string> errors)
        {
            foreach (var feedbackData in feedbacks)
            {
                var trainee = await GetTraineeByEmail(feedbackData.Email, errors);
                if (trainee == null) continue;

                if (!Enum.TryParse<AssessmentType>(feedbackData.PhaseName.Replace(" ", ""), true, out var assessmentType))
                {
                    errors.Add($"Invalid phase name '{feedbackData.PhaseName}' for email {feedbackData.Email}.");
                    continue;
                }

                var feedback = await _feedbackRepository.FirstOrDefaultAsync(f => f.TraineeId == trainee.Id && f.AssessmentType == assessmentType) ??
                               await _feedbackRepository.AddAsync(new Feedback { TraineeId = trainee.Id, AssessmentType = assessmentType });

                foreach (var feedbackItem in feedbackData.FeedbackItems)
                {
                    var header = await _feedbackHeaderRepository.FirstOrDefaultAsync(h => h.Header == feedbackItem.Key) ??
                                 await _feedbackHeaderRepository.AddAsync(new FeedbackHeader { Header = feedbackItem.Key });

                    var response = await _feedbackHeaderResponseRepository.FirstOrDefaultAsync(r => r.FeedbackId == feedback.Id && r.FeedbackHeaderId == header.Id);
                    if (response == null)
                    {
                        await _feedbackHeaderResponseRepository.AddAsync(new FeedbackHeaderResponse { FeedbackId = feedback.Id, FeedbackHeaderId = header.Id, Text = feedbackItem.Value });
                    }
                    else
                    {
                        response.Text = feedbackItem.Value;
                        await _feedbackHeaderResponseRepository.UpdateAsync(response);
                    }
                }
            }
        }
        
        private async Task<Trainee> GetTraineeByEmail(string email, List<string> errors)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                errors.Add($"User with email '{email}' not found.");
                return null;
            }
            var trainee = await _traineeRepository.GetByUserIdAsync(user.Id);
            if (trainee == null)
            {
                errors.Add($"Trainee profile for email '{email}' not found.");
                return null;
            }
            return trainee;
        }
        
        private async Task CreateOrUpdateResult(int traineeId, int assessmentId, int? score)
        {
            if (!score.HasValue) return;

            var result = await _resultRepository.FirstOrDefaultAsync(r => r.TraineeId == traineeId && r.AssessmentId == assessmentId);
            if (result == null)
            {
                await _resultRepository.AddAsync(new Result { TraineeId = traineeId, AssessmentId = assessmentId, ObtainedMark = score.Value });
            }
            else
            {
                result.ObtainedMark = score.Value;
                await _resultRepository.UpdateAsync(result);
            }
        }

        private async Task<Dictionary<AssessmentType, Assessment>> GetAssessments()
        {
            var assessments = new Dictionary<AssessmentType, Assessment>();
            foreach (AssessmentType type in Enum.GetValues(typeof(AssessmentType)))
            {
                var assessment = await _assessmentRepository.FirstOrDefaultAsync(a => a.Type == type) ??
                                 await _assessmentRepository.AddAsync(new Assessment { Type = type, MaxMark = 100 });
                assessments[type] = assessment;
            }
            return assessments;
        }
    }
}