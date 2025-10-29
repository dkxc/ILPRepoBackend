using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class BatchProfile : Profile
    {
        public BatchProfile()
        {
            CreateMap<Batch, BatchDto>().ReverseMap();
        }
    }
}
