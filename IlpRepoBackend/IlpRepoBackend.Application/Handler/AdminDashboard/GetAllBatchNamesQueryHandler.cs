using AutoMapper;
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
    public class GetAllBatchNamesQueryHandler : IRequestHandler<GetAllBatchNamesQuery, List<BatchNameDto>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IMapper _mapper;

        public GetAllBatchNamesQueryHandler(IBatchRepository batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }

        public async Task<List<BatchNameDto>> Handle(GetAllBatchNamesQuery request, CancellationToken cancellationToken)
        {
            var batches = await _batchRepository.GetAllAsync();

            return batches.Select(b => new BatchNameDto
            {
                BatchId = b.Id,
                BatchName = b.BatchName
            }).ToList();
        }
    }
}
