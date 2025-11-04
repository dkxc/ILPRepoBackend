using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Commands.Results
{
    public class UploadResultsCommand : IRequest<ApiResponse<string>>
    {
        public List<TraineeScoreUploadDto> Scores { get; set; } = new();
        public List<TraineeFeedbackUploadDto> Feedbacks { get; set; } = new();
    }

    public class TraineeScoreUploadDto
    {
        public string Email { get; set; }
        public int? TechFundamentalsScore { get; set; }
        public int? SpecializationScore { get; set; }
        public int? BusinessOrientationScore { get; set; }
    }

    public class TraineeFeedbackUploadDto
    {
        public string Email { get; set; }
        public string PhaseName { get; set; } // e.g., "Tech Fundamentals"
        public Dictionary<string, string> FeedbackItems { get; set; } = new();
    }
}