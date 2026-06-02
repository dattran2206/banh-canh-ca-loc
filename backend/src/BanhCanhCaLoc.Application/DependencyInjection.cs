using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace BanhCanhCaLoc.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(Common.Behaviors.LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(Common.Behaviors.ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(Common.Behaviors.TransactionBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
