using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Batchs
{
    public class CreateBatchCommand :IRequest<BatchDto>
    {
        public int Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public string? BatchType { get; set; }
        public BatchStatus Status { get; set; } = BatchStatus.NotStarted;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
