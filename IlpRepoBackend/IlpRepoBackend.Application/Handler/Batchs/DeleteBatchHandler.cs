using IlpRepoBackend.Application.Command.Batchs;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Batchs
{
    public class DeleteBatchHandler : IRequestHandler<DeleteBatchCommand, bool>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly ITraineeRepository _traineeRepository;
        
        public DeleteBatchHandler(
            IBatchRepository batchRepository,
            ITraineeRepository traineeRepository)
        {
            _batchRepository = batchRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<bool> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
        {
            // Properly await the async operation
            var batch = await _batchRepository.GetByIdAsync(request.Id);
            if (batch == null)
            {
                throw new InvalidOperationException($"Batch with ID '{request.Id}' not found");
            }
            
            // Check if batch has trainees
            var traineesInBatch = await _traineeRepository.GetByBatchIdAsync(request.Id);
            if (traineesInBatch.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete batch with ID '{request.Id}' because it has {traineesInBatch.Count()} trainee(s) associated with it. " +
                    "Please remove or reassign the trainees before deleting the batch.");
            }
            
            // Check if batch has phases
            if (batch.Phases != null && batch.Phases.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete batch with ID '{request.Id}' because it has {batch.Phases.Count} phase(s) associated with it. " +
                    "Please remove the phases before deleting the batch.");
            }
            
            // Check if batch has training schedules
            if (batch.TrainingSchedules != null && batch.TrainingSchedules.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete batch with ID '{request.Id}' because it has {batch.TrainingSchedules.Count} training schedule(s) associated with it. " +
                    "Please remove the training schedules before deleting the batch.");
            }
            
            return await _batchRepository.DeleteAsync(request.Id);
        }
    }
}
