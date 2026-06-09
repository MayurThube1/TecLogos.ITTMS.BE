using TecLogos.ITTMS.API.Extensions;

namespace TecLogos.ITTMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // OpenAPI / Swagger (disabled)
            // builder.Services.AddLocalOpenApi();
            // Application services (DB, Repositories, BLL)
            builder.Services.AddApplicationServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Swagger middleware disabled.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = app.MapControllers();

            app.Run();
        }
    }
}