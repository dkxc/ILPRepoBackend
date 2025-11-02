using IlpRepoBackend.Domain.Entities;
using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using IlpRepoBackend.Infrastructure.Repositories;
using IlpRepoBackend.Infrastructure.Service;
using IlpRepoBackend.Infrastructure.Services;
using IlpRepoBackend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Infrastructure
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<ITraineeRepository, TraineeRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IProjecTeamRepository, ProjecTeamRepository>();
            services.AddScoped<IPocForAProjectRepository, PocForAProjectRepository>();
            services.AddScoped<IMentorRepository, MenterRepository>();
            services.AddScoped<IMentorForPRojectRepository, MentorForPRojectRepository>();
            services.AddScoped<IPocRepository, PocRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProjectLinkRepository, ProjectLinkRepository>();
            services.AddScoped<ILinkRepository, LinkRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentRequestRepository, DocumentRequestRepository>();
            services.AddScoped<IDocumentSubmissionRepository, DocumentSubmissionRepository>();
            services.AddScoped<IFileStorageService, SupabaseStorageService>();

            return services;
        }
    }
}
