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
    public class CreateBatchTypeHandler : IRequestHandler<CreateBatchTypeCommand, ApiResponse<BatchTypeDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreateBatchTypeHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<BatchTypeDto>> Handle(CreateBatchTypeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<BatchTypeDto>.Fail("Name is required");

            // prevent duplicate names (case-insensitive)
            if (await _batchRepository.BatchTypeNameExistsAsync(request.Name))
                return ApiResponse<BatchTypeDto>.Fail("Batch type with the same name already exists");

            var batchType = new BatchType
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _batchRepository.AddBatchTypeAsync(batchType);
            var dto = _mapper.Map<BatchTypeDto>(created);
            return ApiResponse<BatchTypeDto>.Created(dto);
        }
    }
}