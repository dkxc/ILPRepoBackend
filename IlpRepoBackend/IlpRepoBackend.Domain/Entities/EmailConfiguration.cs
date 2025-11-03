using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class EmailConfiguration
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public int DaysBeforeDueDate { get; set; }
        public TimeSpan ScheduledTime { get; set; } = new TimeSpan(9, 0, 0); 
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBodyTemplate { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
