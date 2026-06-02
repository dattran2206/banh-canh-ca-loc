using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BanhCanhCaLoc.Domain.Repositories;
using BanhCanhCaLoc.Application.Common.Interfaces.Services;
using BanhCanhCaLoc.Infrastructure.Persistence;
using BanhCanhCaLoc.Infrastructure.Repositories;
using BanhCanhCaLoc.Infrastructure.Services;

namespace BanhCanhCaLoc.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BanhCanhCaLocDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
