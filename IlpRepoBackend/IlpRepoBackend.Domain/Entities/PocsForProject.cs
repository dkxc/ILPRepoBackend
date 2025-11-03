using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class PocsForProject
    {
        public int Id { get; set; }
        public int PocId { get; set; }
        public int ProjectId { get; set; }

        public Poc Poc { get; set; }           // Renamed from 'poc'
        public Project Project { get; set; }   // Renamed from 'project'
    }

}
