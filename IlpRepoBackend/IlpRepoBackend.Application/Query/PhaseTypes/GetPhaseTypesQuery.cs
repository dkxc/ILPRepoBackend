using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.PhaseTypes
{
    public class GetPhaseTypesQuery : IRequest<ApiResponse<List<PhaseTypeDto>>>
    {
    }
}