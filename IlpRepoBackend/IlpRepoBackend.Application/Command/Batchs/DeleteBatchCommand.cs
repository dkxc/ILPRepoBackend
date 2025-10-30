using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Batchs
{
    public class DeleteBatchCommand :IRequest<bool>
    {
        public int Id { get; set; }
    }
}
