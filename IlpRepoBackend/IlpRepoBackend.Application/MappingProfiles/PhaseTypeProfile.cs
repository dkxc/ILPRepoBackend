using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class PhaseTypeProfile : Profile
    {
        public PhaseTypeProfile()
        {
            CreateMap<PhaseType, PhaseTypeDto>();
        }
    }
}