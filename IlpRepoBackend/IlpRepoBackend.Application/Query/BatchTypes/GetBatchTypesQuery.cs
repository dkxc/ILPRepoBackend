using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Query.BatchTypes
{
    public class GetBatchTypesQuery : IRequest<ApiResponse<List<BatchTypeDto>>> { }
}