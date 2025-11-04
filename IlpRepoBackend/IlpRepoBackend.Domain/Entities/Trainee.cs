using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Domain.Entities
{
    public class Trainee
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Email { get; set; }
        public int BatchId { get; set; }
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public Batch Batch { get; set; } = null!;
        public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
        public ICollection<Result> Results { get; set; } = new List<Result>();
        public Feedback? Feedback { get; set; }
        public ICollection<TraineeActivity> TraineeActivities { get; set; } = new List<TraineeActivity>();
        public ICollection<BoPhase> BoPhases { get; set; } = new List<BoPhase>();
        public ICollection<TraineeDu> TraineeDus { get; set; } = new List<TraineeDu>();
    }
}
