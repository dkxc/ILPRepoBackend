<<<<<<< HEAD
using IlpRepoBackend.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
=======
﻿using FluentValidation;
using IlpRepoBackend.Application.Common.Behaviour;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> b1a77052b2965f66ef4845525863870bf757b159

namespace IlpRepoBackend.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
<<<<<<< HEAD
            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register Authorization Service
            services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();

            return services;
=======
            services.AddAutoMapper(typeof(ApplicationServiceRegistration).Assembly);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));
            // FluentValidation - register all validators in this assembly
            services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
            // MediatR Pipeline Behavior for validation
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            return services;

>>>>>>> b1a77052b2965f66ef4845525863870bf757b159
        }
    }
}
