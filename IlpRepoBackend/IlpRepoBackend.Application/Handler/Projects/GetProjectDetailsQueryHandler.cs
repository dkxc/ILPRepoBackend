using AutoMapper;
using IlpRepoBackend.Application.CustomException;
using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Query;
using IlpRepoBackend.Application.Wrappers;
using IlpRepoBackend.Domain.Persistence;
using MediatR;

namespace IlpRepoBackend.Application.Handler.Projects
{
    public class GetProjectDetailsQueryHandler : IRequestHandler<GetProjectDetailsQuery, ApiResponse<ProjectDetailsDto>>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public GetProjectDetailsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProjectDetailsDto>> Handle(GetProjectDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _projectRepository.GetProjectDetailsAsync(request.ProjectId);

                if (project == null)
                {
                    throw new NotFoundException("Project", request.ProjectId);
                }

                var projectDetailsDto = _mapper.Map<ProjectDetailsDto>(project);

                return new ApiResponse<ProjectDetailsDto>(projectDetailsDto, "Project details retrieved successfully");
            }
            catch (NotFoundException ex)
            {
                return new ApiResponse<ProjectDetailsDto>(ex.Message, 404);
            }
            catch (Exception ex)
            {
                return new ApiResponse<ProjectDetailsDto>($"An error occurred: {ex.Message}", 500);
            }
        }
    }
}
