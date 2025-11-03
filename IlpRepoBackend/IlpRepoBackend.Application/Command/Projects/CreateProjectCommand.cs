using IlpRepoBackend.Application.Dto.Project;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class CreateProjectCommand : IRequest<ApiResponse<ProjectDto>>
    {
        public CreateProjectDto ProjectData { get; set; }

        public CreateProjectCommand(CreateProjectDto projectData)
        {
            ProjectData = projectData;
        }
    }
}
