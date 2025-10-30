using AutoMapper;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Domain.Entities;

namespace IlpRepoBackend.Application.MappingProfile
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<Project, ProjectDetailsDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.BatchNames, opt => opt.MapFrom(src => 
                    src.ProjectTeams.Select(pt => pt.Trainee.Batch.BatchName).Distinct().ToList()))
                .ForMember(dest => dest.Trainees, opt => opt.MapFrom(src => 
                    src.ProjectTeams.Select(pt => new TraineeDto
                    {
                        Id = pt.Trainee.Id,
                        Username = pt.Trainee.User.Username,
                        Email = pt.Trainee.Email ?? string.Empty,
                        PhoneNo = pt.Trainee.PhoneNo,
                        Role = pt.Role.ToString()
                    }).ToList()))
                .ForMember(dest => dest.NoOfTrainees, opt => opt.MapFrom(src => src.ProjectTeams.Count))
                .ForMember(dest => dest.TechStack, opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Technology) 
                        ? src.Technology.Split(new[] { ',' }, StringSplitOptions.None).Select(t => t.Trim()).ToList() 
                        : new List<string>()))
                .ForMember(dest => dest.ProjectLinks, opt => opt.MapFrom(src => 
                    src.ProjectLinks.Select(pl => new ProjectLinkDto
                    {
                        Id = pl.Id,
                        LinkUrl = pl.LinkUrl,
                        LinkType = pl.Link != null ? pl.Link.Name : null
                    }).ToList()));

            CreateMap<ProjectTeam, TraineeDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Trainee.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Trainee.User.Username))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Trainee.Email ?? string.Empty))
                .ForMember(dest => dest.PhoneNo, opt => opt.MapFrom(src => src.Trainee.PhoneNo))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<ProjectLink, ProjectLinkDto>()
                .ForMember(dest => dest.LinkType, opt => opt.MapFrom(src => src.Link != null ? src.Link.Name : null));
        }
    }
}
