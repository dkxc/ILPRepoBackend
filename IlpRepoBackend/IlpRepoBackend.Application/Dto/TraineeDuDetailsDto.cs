using System;
using System.Collections.Generic;

namespace IlpRepoBackend.Application.Dto
{
    public class TraineeDuDetailsDto
    {
        public int TraineeDuId { get; set; }
        public string? TraineeName { get; set; }
        public string? DuAllocated { get; set; }
        public string? Location { get; set; }
        public string? OjtMentor { get; set; }
    }
}