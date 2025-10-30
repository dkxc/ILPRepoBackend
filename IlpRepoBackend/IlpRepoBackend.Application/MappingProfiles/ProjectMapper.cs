using AutoMapper;
using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Domain.Entities;
using System.Linq;

namespace IlpRepoBackend.Application.Mapping
{
    public class ProjectMappingProfile : Profile
    {
        public ProjectMappingProfile()
        {
            // Project Entity -> ProjectDto
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.ProjectName))
                .ForMember(dest => dest.Technology, opt => opt.MapFrom(src => src.Technology))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.Progress))
                .ForMember(dest => dest.BatchId, opt => opt.Ignore()) // Set manually if needed
                .ForMember(dest => dest.BatchName, opt => opt.Ignore()) // Set manually if needed
                .ForMember(dest => dest.TeamMembers, opt => opt.MapFrom(src =>
                    src.ProjectTeams != null
                        ? src.ProjectTeams
                            .Where(pt => pt.Role == Domain.Enum.ProjectRole.Trainee &&
                                        pt.Trainee != null &&
                                        pt.Trainee.User != null)
                            .Select(pt => pt.Trainee.User.Username)
                            .ToList()
                        : new List<string>()))
                .ForMember(dest => dest.Mentors, opt => opt.MapFrom(src =>
                    src.MentersForProjects != null
                        ? src.MentersForProjects
                            .Where(m => m.Mentor != null)
                            .Select(m => m.Mentor)
                            .ToList()
                        : new List<Mentor>()))
                .ForMember(dest => dest.Pocs, opt => opt.MapFrom(src =>
                    src.PocsForProjects != null
                        ? src.PocsForProjects
                            .Where(p => p.Poc != null)
                            .Select(p => p.Poc)
                            .ToList()
                        : new List<Poc>()))
                .ForMember(dest => dest.documentRequest, opt => opt.MapFrom(src =>
                    src.DocumentRequests != null
                        ? src.DocumentRequests.ToList()
                        : new List<DocumentRequest>()))
                .ForMember(dest => dest.documentSubmissions, opt => opt.MapFrom(src =>
                    src.DocumentRequests != null
                        ? src.DocumentRequests
                            .SelectMany(dr => dr.DocumentSubmissions ?? new List<DocumentSubmission>())
                            .ToList()
                        : new List<DocumentSubmission>()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // Reverse mapping if needed
            CreateMap<ProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectTeams, opt => opt.Ignore())
                .ForMember(dest => dest.MentersForProjects, opt => opt.Ignore())
                .ForMember(dest => dest.PocsForProjects, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectLinks, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentRequests, opt => opt.Ignore());
        }
    }
}