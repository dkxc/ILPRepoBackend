using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using IlpRepoBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace IlpRepoBackend.Infrastructure
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register Specific Repositories
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IDocumentRequestRepository, DocumentRequestRepository>();
            services.AddScoped<ILinkRepository, LinkRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IDocumentSubmissionRepository, DocumentSubmissionRepository>();

            return services;
        }
    }
}
