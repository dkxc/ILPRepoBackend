using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class CurriculumProfile : Profile
    {
        public CurriculumProfile()
        {
            CreateMap<Curriculum, CurriculumDto>().ReverseMap();
            CreateMap<CreateCurriculumDto, Curriculum>();
            CreateMap<UpdateCurriculumDto, Curriculum>();
        }
    }
}
