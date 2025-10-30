<<<<<<< HEAD
=======
﻿using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
namespace IlpRepoBackend.Application.Dto
{
    public class TraineeDto
    {
        public int Id { get; set; }
<<<<<<< HEAD
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNo { get; set; }
        public string Role { get; set; } = string.Empty;
=======
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
    }
}
