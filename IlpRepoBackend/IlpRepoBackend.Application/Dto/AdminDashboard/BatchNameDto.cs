using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Dto
{
    public class BatchNameDto
    {
        public int BatchId { get; set; }
        public string BatchName { get; set; } = string.Empty;
    }
}
