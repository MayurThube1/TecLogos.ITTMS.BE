using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.BLL.Services;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Repositories;

namespace TecLogos.ITTMS.API.Extensions
{
    public static class ServiceExtensions
    {
        // Registers OpenAPI (Swagger) and application services.
        public static IServiceCollection AddOpenApi(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TecLogos ITTMS API", Version = "v1" });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // DB
            services.AddSingleton<DBConnection>();

            // Repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            // BLL Services
            services.AddScoped<IEmployeeService, EmployeeService>();

            return services;
        }
    }
}
