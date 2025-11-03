using IlpRepoBackend.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.Batchs
{
    public class CreateBatchCommand : IRequest<BatchDto>
    {
        public int Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public int? BatchTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<PhaseDto> Phases { get; set; } = new List<PhaseDto>();
    }
}
