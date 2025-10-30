using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Command.Projects
{
    public class DeleteProjectCommand : IRequest<ApiResponse<bool>>
    {
        public int ProjectId { get; set; }

        public DeleteProjectCommand(int projectId)
        {
            ProjectId = projectId;
        }
    }
}
