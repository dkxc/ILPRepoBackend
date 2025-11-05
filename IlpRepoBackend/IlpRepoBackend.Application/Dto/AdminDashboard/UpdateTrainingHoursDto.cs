using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto.AdminDashboard
{
    public class UpdateTrainingHoursDto
    {
        public int BatchId { get; set; }
        public DateTime TrainingDate { get; set; }
        public int Hours { get; set; }
    }
}
