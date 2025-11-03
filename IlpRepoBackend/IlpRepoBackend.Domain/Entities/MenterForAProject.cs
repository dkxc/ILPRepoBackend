using IlpRepoBackend.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Entities
{
    public class MenterForAProject
    {
        public int Id { get; set; }
        public int MenterId { get; set; }
        public int ProjectId { get; set; }
        public MentorType MentorType { get; set; }

        public Project Project { get; set; }  // Renamed from 'project'
        public Mentor Mentor { get; set; }
    }

}
