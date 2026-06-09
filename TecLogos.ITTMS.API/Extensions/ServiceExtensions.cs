using Microsoft.Extensions.DependencyInjection;
// OpenAPI types removed because Swagger generation is disabled here
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.BLL.Services;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.DAL.Repositories;

namespace TecLogos.ITTMS.API.Extensions
{
    public static class ServiceExtensions
    {
        // Registers OpenAPI (Swagger) and application services.
        public static IServiceCollection AddLocalOpenApi(this IServiceCollection services)
        {
            // Keep API explorer registration, but do not register Swagger generation here.
            services.AddEndpointsApiExplorer();

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