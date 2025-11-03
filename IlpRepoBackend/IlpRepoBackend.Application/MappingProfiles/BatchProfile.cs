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
            CreateMap<Batch, BatchDto>()
                .ForMember(dest => dest.BatchTypeId, opt => opt.MapFrom(src => src.BatchTypeId))
                .ForMember(dest => dest.BatchTypeName, opt => opt.MapFrom(src => src.BatchType != null ? src.BatchType.Name : null))
                .ForMember(dest => dest.Phases, opt => opt.MapFrom(src => src.Phases))
                .ReverseMap();

            CreateMap<Phase, PhaseDto>()
                .ForMember(dest => dest.PhaseTypeId, opt => opt.MapFrom(src => src.PhaseTypeId))
                .ReverseMap();
        }
    }
}
