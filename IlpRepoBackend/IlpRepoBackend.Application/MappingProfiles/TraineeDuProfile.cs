using AutoMapper;
using IlpRepoBackend.Application.Command.TraineeDus;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class TraineeDuProfile : Profile
    {
        public TraineeDuProfile()
        {
            CreateMap<TraineeDu, TraineeDuDto>()
                .ForMember(dest => dest.TraineeName, opt => opt.MapFrom(src => src.Trainee != null ? src.Trainee.User.Username : null))
                .ForMember(dest => dest.DuName, opt => opt.MapFrom(src => src.Du != null ? src.Du.Name : null));

            CreateMap<CreateTraineeDuCommand, TraineeDu>();
            CreateMap<AddTraineeDuForBatchDto, TraineeDu>();
        }
    }
}