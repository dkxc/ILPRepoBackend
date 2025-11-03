using AutoMapper;
using IlpRepoBackend.Application.Command.PhaseTypes;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.PhaseTypes
{
    public class UpdatePhaseTypeHandler : IRequestHandler<UpdatePhaseTypeCommand, ApiResponse<PhaseTypeDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public UpdatePhaseTypeHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PhaseTypeDto>> Handle(UpdatePhaseTypeCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return ApiResponse<PhaseTypeDto>.Fail("Invalid id");

            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<PhaseTypeDto>.Fail("Name is required");

            // Check duplicate excluding current id
            if (await _batchRepository.PhaseTypeNameExistsAsync(request.Name, request.Id))
                return ApiResponse<PhaseTypeDto>.Fail("Phase type with the same name already exists");

            var phaseType = new PhaseType
            {
                Id = request.Id,
                Name = request.Name,
                UpdatedAt = DateTime.UtcNow
            };

            var updated = await _batchRepository.UpdatePhaseTypeAsync(phaseType);
            if (updated == null) 
                return ApiResponse<PhaseTypeDto>.Fail("Phase type not found");

            var dto = _mapper.Map<PhaseTypeDto>(updated);
            return ApiResponse<PhaseTypeDto>.Success(dto, "Updated successfully");
        }
    }
}