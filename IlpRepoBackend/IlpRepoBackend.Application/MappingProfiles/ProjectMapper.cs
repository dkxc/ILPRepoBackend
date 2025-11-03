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
            // Create mappings for nested DTOs first
            CreateMap<Mentor, MentorDto>();
            CreateMap<Poc, PocDto>();
            CreateMap<DocumentRequest, DocumentRequestDto>()
                .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Document != null ? src.Document.Name : null));
            CreateMap<DocumentSubmission, DocumentSubmissionDto>();

            // Project Entity -> ProjectDto
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.BatchId, opt => opt.MapFrom(src =>
                    src.ProjectTeams != null && src.ProjectTeams.Any() && src.ProjectTeams.First().Trainee != null
                        ? src.ProjectTeams.First().Trainee.BatchId
                        : 0))
                .ForMember(dest => dest.BatchName, opt => opt.MapFrom(src =>
                    src.ProjectTeams != null && src.ProjectTeams.Any() &&
                    src.ProjectTeams.First().Trainee != null &&
                    src.ProjectTeams.First().Trainee.Batch != null
                        ? src.ProjectTeams.First().Trainee.Batch.BatchName ?? string.Empty
                        : string.Empty))
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
                .ForMember(dest => dest.DocumentRequests, opt => opt.MapFrom(src =>
                    src.DocumentRequests != null
                        ? src.DocumentRequests.ToList()
                        : new List<DocumentRequest>()))
                .ForMember(dest => dest.DocumentSubmissions, opt => opt.MapFrom(src =>
                    src.DocumentRequests != null
                        ? src.DocumentRequests
                            .SelectMany(dr => dr.DocumentSubmissions ?? new List<DocumentSubmission>())
                            .ToList()
                        : new List<DocumentSubmission>()))
                .ForMember(dest => dest.TeamLead, opt => opt.MapFrom(src =>
                    src.ProjectTeams != null
                        ? src.ProjectTeams
                            .Where(pt => pt.Role == Domain.Enum.ProjectRole.TeamLead &&
                                        pt.Trainee != null &&
                                        pt.Trainee.User != null)
                            .Select(pt => pt.Trainee.User.Username)
                            .FirstOrDefault()
                        : null))
                .ForMember(dest => dest.ScrumMaster, opt => opt.MapFrom(src =>
                    src.ProjectTeams != null
                        ? src.ProjectTeams
                            .Where(pt => pt.Role == Domain.Enum.ProjectRole.ScrumMaster &&
                                        pt.Trainee != null &&
                                        pt.Trainee.User != null)
                            .Select(pt => pt.Trainee.User.Username)
                            .FirstOrDefault()
                        : null));

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