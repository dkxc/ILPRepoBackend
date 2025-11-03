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
    public class CreatePhaseTypeHandler : IRequestHandler<CreatePhaseTypeCommand, ApiResponse<PhaseTypeDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public CreatePhaseTypeHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PhaseTypeDto>> Handle(CreatePhaseTypeCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return ApiResponse<PhaseTypeDto>.Fail("Name is required");

            // Prevent duplicate names (case-insensitive)
            if (await _batchRepository.PhaseTypeNameExistsAsync(request.Name))
                return ApiResponse<PhaseTypeDto>.Fail("Phase type with the same name already exists");

            var phaseType = new PhaseType
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _batchRepository.AddPhaseTypeAsync(phaseType);
            var dto = _mapper.Map<PhaseTypeDto>(created);
            return ApiResponse<PhaseTypeDto>.Created(dto);
        }
    }
}