using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto
{
    public class AddTraineeForABatchDto
    {
        //public int Id { get; set; }
        //public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        //public int BatchId { get; set; }
        //public string BatchName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNo { get; set; }
        public TraineeStatus Status { get; set; } = TraineeStatus.Active;
        public BloodGroup? BloodGroup { get; set; }
        public string? AadhaarId { get; set; }
        public string? HealthCondition { get; set; }
        public string? PersonalInterest { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactNo { get; set; }
        public string Password { get;  set; }
    }
}
