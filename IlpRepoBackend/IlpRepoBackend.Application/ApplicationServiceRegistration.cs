using IlpRepoBackend.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IlpRepoBackend.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register Authorization Service
            services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

            return services;
        }
    }
}
