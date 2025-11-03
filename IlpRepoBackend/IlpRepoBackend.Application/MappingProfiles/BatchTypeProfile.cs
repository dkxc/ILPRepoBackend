using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfiles
{
    public class BatchTypeProfile : Profile
    {
        public BatchTypeProfile()
        {
            CreateMap<BatchType, BatchTypeDto>().ReverseMap();
        }
    }
}
