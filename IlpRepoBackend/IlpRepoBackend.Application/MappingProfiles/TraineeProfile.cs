using AutoMapper;
using IlpRepoBackend.Application.Command.Trainees;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class TraineeProfile : Profile
    {
        public TraineeProfile()
        {
            CreateMap<Trainee, TraineeDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.BatchName, opt => opt.MapFrom(src => src.Batch.BatchName));
            CreateMap<CreateTraineeCommand, Trainee>();
            CreateMap<AddTraineeForABatchDto, Trainee>().ReverseMap();
            CreateMap<AddTraineeForABatchDto, Trainee>();
        }
    }
}
