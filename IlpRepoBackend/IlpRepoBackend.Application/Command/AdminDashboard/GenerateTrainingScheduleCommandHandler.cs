using IlpRepoBackend.Application.Command.AdminDashboard;
using IlpRepoBackend.Domain.Persistence;
using AutoMapper;
using IlpRepoBackend.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.AdminDashboard
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

            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
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





