using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Enum;
using MediatR;

namespace IlpRepoBackend.Application.Command.Trainees
{
    public class CreateTraineeCommand : IRequest<ApiResponse<TraineeDto>>
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public TraineeStatus Status { get; set; } = TraineeStatus.Active;
        public BloodGroup? BloodGroup { get; set; }
        public string? AadhaarId { get; set; }
        public string? HealthCondition { get; set; }
        public string? PersonalInterest { get; set; }
        public string? Address { get; set; }
        public string? CurrentAddress { get; set; }  // New field
        public string? ContactNumber { get; set; }   // New field
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactNo { get; set; }
        public UserRole Role { get; internal set; }
        public bool IsActive { get; internal set; }
    }
}
