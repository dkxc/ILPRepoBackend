using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Enum;
using MediatR;
using System;
using System.Text.Json.Serialization;

namespace IlpRepoBackend.Application.Command.Trainees
{
    public class UpdateTraineeCommand : IRequest<ApiResponse<TraineeDto>>
    {
        [JsonIgnore]  // Prevent this from being set via JSON body
        public int Id { get; set; }
        
        public string? Username { get; set; }  // Added to allow updating trainee name
        public string? Email { get; set; }
        public string? PhoneNo { get; set; }
        public TraineeStatus Status { get; set; }
        public BloodGroup? BloodGroup { get; set; }
        public string? AadhaarId { get; set; }
        public string? HealthCondition { get; set; }
        public string? PersonalInterest { get; set; }
        public string? Address { get; set; }
        public string? CurrentAddress { get; set; }
        public string? ContactNumber { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactNo { get; set; }
    }
}