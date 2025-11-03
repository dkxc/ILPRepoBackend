using AutoMapper;
using IlpRepoBackend.Application.Command.BoPhases;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class BoPhaseProfile : Profile
    {
        public BoPhaseProfile()
        {
            CreateMap<BoPhase, BoPhaseDto>()
                .ForMember(dest => dest.TraineeName, opt => opt.MapFrom(src => src.Trainee != null ? src.Trainee.User.Username : null))
                .ForMember(dest => dest.BuddyName, opt => opt.MapFrom(src => src.Buddy != null ? src.Buddy.Name : null));

            CreateMap<CreateBoPhaseCommand, BoPhase>();
            CreateMap<AddBoPhaseForBatchDto, BoPhase>();
        }
    }
}