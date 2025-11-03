using IlpRepoBackend.Application.Dto.Links;
using IlpRepoBackend.Application.Query.Links;
using IlpRepoBackend.Application.Wrapper;
using IlpRepoBackend.Domain.Persistence;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IlpRepoBackend.Application.Handler.Links
{
    public class GetLinkTypesByBatchIdHandler : IRequestHandler<GetLinkTypesByBatchIdQuery, ApiResponse<List<BatchLinkTypeDto>>>
    {
        private readonly IBatchRepository _batchRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectLinkRepository _projectLinkRepository;
        private readonly ILinkRepository _linkRepository;

        public GetLinkTypesByBatchIdHandler(
            IBatchRepository batchRepository,
            IProjectRepository projectRepository,
            IProjectLinkRepository projectLinkRepository,
            ILinkRepository linkRepository)
        {
            _batchRepository = batchRepository;
            _projectRepository = projectRepository;
            _projectLinkRepository = projectLinkRepository;
            _linkRepository = linkRepository;
        }

        public async Task<ApiResponse<List<BatchLinkTypeDto>>> Handle(GetLinkTypesByBatchIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Verify batch exists
                var batch = await _batchRepository.GetByIdAsync(request.BatchId);
                if (batch == null)
                {
                    return ApiResponse<List<BatchLinkTypeDto>>.Fail($"Batch with ID {request.BatchId} not found");
                }

                // 2. Get all projects in this batch
                var projectsInBatch = await _projectRepository.GetProjectsByBatchIdAsync(request.BatchId);
                
                if (!projectsInBatch.Any())
                {
                    return ApiResponse<List<BatchLinkTypeDto>>.Success(
                        new List<BatchLinkTypeDto>(), 
                        $"No projects found for batch {request.BatchId}");
                }

                var projectIds = projectsInBatch.Select(p => p.Id).ToList();

                // 3. Get all project links for projects in this batch
                var allProjectLinks = new List<Domain.Entities.ProjectLink>();
                foreach (var projectId in projectIds)
                {
                    var projectLinks = await _projectLinkRepository.GetByProjectIdAsync(projectId);
                    allProjectLinks.AddRange(projectLinks);
                }

                // 4. Group by LinkId to find required link types
                var linkTypeGroups = allProjectLinks
                    .Where(pl => pl.LinkId.HasValue)
                    .GroupBy(pl => pl.LinkId.Value)
                    .ToList();

                var batchLinkTypes = new List<BatchLinkTypeDto>();

                // 5. Process each link type
                foreach (var group in linkTypeGroups)
                {
                    var linkTypeId = group.Key;
                    
                    // Get link type details
                    var linkType = await _linkRepository.GetByIdAsync(linkTypeId);
                    if (linkType == null) continue;

                    // Calculate statistics
                    var projectLinksForType = group.ToList();
                    var totalProjects = projectIds.Count;
                    var submittedProjects = projectLinksForType
                        .Count(pl => !string.IsNullOrEmpty(pl.LinkUrl));
                    var pendingProjects = totalProjects - submittedProjects;
                    var completionPercentage = totalProjects > 0 ? 
                        Math.Round((double)submittedProjects / totalProjects * 100, 1) : 0;

                    batchLinkTypes.Add(new BatchLinkTypeDto
                    {
                        LinkTypeId = linkTypeId,
                        LinkTypeName = linkType.Name ?? "Unknown",
                        TotalProjects = totalProjects,
                        SubmittedProjects = submittedProjects,
                        PendingProjects = pendingProjects,
                        CompletionPercentage = completionPercentage
                    });
                }

                return ApiResponse<List<BatchLinkTypeDto>>.Success(
                    batchLinkTypes,
                    $"Retrieved {batchLinkTypes.Count} link types for batch {request.BatchId}");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BatchLinkTypeDto>>.Fail($"Error retrieving link types for batch: {ex.Message}");
            }
        }
    }
}