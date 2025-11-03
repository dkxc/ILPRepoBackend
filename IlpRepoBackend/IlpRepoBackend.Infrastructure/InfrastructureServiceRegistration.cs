using IlpRepoBackend.Domain.Persistence;
using IlpRepoBackend.Infrastructure.Context;
using IlpRepoBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IlpRepoBackend.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<ITraineeRepository, TraineeRepository>();
            services.AddScoped<ITraineeDuRepository, TraineeDuRepository>();
            services.AddScoped<IDuRepository, DuRepository>();
            services.AddScoped<IBoPhaseRepository, BoPhaseRepository>();
            services.AddScoped<IBuddyRepository, BuddyRepository>();

            return services;
        }
    }
}