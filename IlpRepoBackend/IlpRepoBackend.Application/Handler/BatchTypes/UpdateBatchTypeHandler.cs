using AutoMapper;
using IlpRepoBackend.Application.Command.BatchTypes;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.BatchTypes
{
    public class UpdateBatchTypeHandler : IRequestHandler<UpdateBatchTypeCommand, ApiResponse<BatchTypeDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public UpdateBatchTypeHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BatchTypeDto>> Handle(UpdateBatchTypeCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return ApiResponse<BatchTypeDto>.Fail("Invalid id");

            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<BatchTypeDto>.Fail("Name is required");

            // check duplicate excluding current id
            if (await _batchRepository.BatchTypeNameExistsAsync(request.Name, request.Id))
                return ApiResponse<BatchTypeDto>.Fail("Batch type with the same name already exists");

            var batchType = new BatchType
            {
                Id = request.Id,
                Name = request.Name,
                UpdatedAt = DateTime.UtcNow
            };

            var updated = await _batchRepository.UpdateBatchTypeAsync(batchType);
            if (updated == null) return ApiResponse<BatchTypeDto>.Fail("Batch type not found");

            var dto = _mapper.Map<BatchTypeDto>(updated);
            return ApiResponse<BatchTypeDto>.Success(dto, "Updated successfully");
        }
    }
}
