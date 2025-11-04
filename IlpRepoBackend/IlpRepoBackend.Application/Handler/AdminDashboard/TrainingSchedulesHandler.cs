using IlpRepoBackend.Application.Command.AdminDashboard;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace IlpRepoBackend.Application.Handler.TrainingSchedules
{
    public class GenerateTrainingScheduleCommandHandler : IRequestHandler<GenerateTrainingScheduleCommand, bool>
    {
        private readonly ITrainingScheduleRepository _trainingScheduleRepository;

        public GenerateTrainingScheduleCommandHandler(ITrainingScheduleRepository trainingScheduleRepository)
        {
            _trainingScheduleRepository = trainingScheduleRepository;
        }

        public async Task<bool> Handle(GenerateTrainingScheduleCommand request, CancellationToken cancellationToken)
        {
            var schedules = new List<TrainingSchedule>();

            for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
            {
                schedules.Add(new TrainingSchedule
                {
                    BatchId = request.BatchId,
                    TrainingDate = date,
                    Hours = 8,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _trainingScheduleRepository.AddRangeAsync(schedules);
            return true;
        }
    }
}
