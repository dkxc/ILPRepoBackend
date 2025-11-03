using IlpRepoBackend.Application.Dto;
using IlpRepoBackend.Application.Wrapper;
using MediatR;
using System;

namespace IlpRepoBackend.Application.Command.BoPhases
{
    public class UpdateBoPhaseCommand : IRequest<ApiResponse<BoPhaseDetailsDto>>
    {
        public int BoPhaseId { get; set; }
        public string? TraineeName { get; set; }
        public string BuddyName { get; set; }
        public string? DuName { get; set; }  // Changed from DuId to DuName
    }
}