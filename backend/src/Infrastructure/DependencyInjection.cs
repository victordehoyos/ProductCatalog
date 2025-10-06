using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalogAPI.Application.Ports;
using ProductCatalogAPI.Domain.Interfaces.Read;
using ProductCatalogAPI.Domain.Interfaces.Write;
using ProductCatalogAPI.Infrastructure.Data;
using ProductCatalogAPI.Infrastructure.Persistence;
using ProductCatalogAPI.Infrastructure.Repositories.Read;
using ProductCatalogAPI.Infrastructure.Repositories.Write;
using ProductCatalogAPI.Infrastructure.Services;

namespace ProductCatalogAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        // repositories
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // infrastructure Services
        services.AddScoped<IJwtService, JwtService>();
        
        return services;
    }
}