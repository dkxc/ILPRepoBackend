
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query.AdminDashboard;

using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.AdminDashboard
{
    public class GetBatchDetailQueryHandler : IRequestHandler<GetBatchDetailQuery, BatchDetailDto>
    {
        private readonly IBatchRepository _batchRepo;
        private readonly ITraineeRepository _traineeRepo;
        private readonly IPhaseRepository _phaseRepo;

        public GetBatchDetailQueryHandler(
            IBatchRepository batchRepo,
            ITraineeRepository traineeRepo,
            IPhaseRepository phaseRepo)
        {
            _batchRepo = batchRepo;
            _traineeRepo = traineeRepo;
            _phaseRepo = phaseRepo;
        }

        public async Task<BatchDetailDto> Handle(GetBatchDetailQuery request, CancellationToken cancellationToken)
        {
            // 1️⃣ Fetch batch info
            var batch = await _batchRepo.GetByIdAsync(request.BatchId);
            if (batch == null)
                throw new Exception($"Batch with ID {request.BatchId} not found.");

            // 2️⃣ Count trainees for this batch
            var trainees = await _traineeRepo.GetAllAsync();
            var traineeCount = trainees.Count(t => t.BatchId == batch.Id);

            // 3️⃣ Get all phases related to this batch
            var phases = await _phaseRepo.GetAllAsync();
            var batchPhases = phases
                .Where(p => p.BatchId == batch.Id)
                .Select(p => new BatchPhaseDto
                {
                    PhaseType = p.PhaseType,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate
                })
                .ToList();

            // 4️⃣ Calculate current day of batch excluding Sundays
            int dayOfBatch = CalculateWorkingDays(batch.StartDate ?? DateTime.Now, DateTime.Now);


            // 5️⃣ Build and return DTO
            return new BatchDetailDto
            {
                BatchId = batch.Id,
                BatchName = batch.BatchName,
                BatchStatus = batch.Status.ToString(),
                BatchType = batch.BatchType.ToString(),
                StartDate = batch.StartDate ?? DateTime.MinValue,
                EndDate = batch.EndDate ?? DateTime.MinValue,
                NoOfTrainees = traineeCount,
                DayOfBatch = dayOfBatch,
                Phases = batchPhases
            };
        }

        // Helper function to calculate days excluding Sundays
        private static int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate) return 0;

            int totalDays = 0;
            for (DateTime day = startDate; day <= endDate; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }
            return totalDays;
        }
    }
}
