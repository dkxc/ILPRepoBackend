using IlpRepoBackend.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Command.Batchs
{
    public class UpdateBatchCommand : IRequest<BatchDto>
    {
        public int Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public int? BatchTypeId { get; set; }
        // Status is auto-calculated based on StartDate and EndDate - not provided by user
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<PhaseDto> Phases { get; set; } = new List<PhaseDto>();
    }
}
