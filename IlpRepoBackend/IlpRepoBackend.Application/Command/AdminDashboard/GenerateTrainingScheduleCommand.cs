using IlpRepoBackend.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.AdminDashboard
{
    public class GenerateTrainingScheduleCommand: IRequest<bool>
    {
     public int BatchId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GenerateTrainingScheduleCommand(int batchId, DateTime startDate, DateTime endDate)
    {
        BatchId = batchId;
        StartDate = startDate;
        EndDate = endDate;
    }
}

   
}



