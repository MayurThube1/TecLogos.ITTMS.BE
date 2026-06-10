using TecLogos.ITTMS.API.Extensions;
using TecLogos.ITTMS.API.Middleware;

namespace TecLogos.ITTMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Register application services (DI for repos, services, DB)
            // OpenAPI / Swagger (disabled)
            // builder.Services.AddLocalOpenApi();
            // Application services (DB, Repositories, BLL)
            builder.Services.AddApplicationServices();

            // Configure JWT authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Configure CORS for React frontend
            builder.Services.AddCorsPolicy();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            // Swagger middleware disabled.

            // Global exception handler (must be first in pipeline)
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            // CORS must come before auth
            app.UseCors("AllowFrontend");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom JWT middleware (attaches user claims to HttpContext.Items)
            app.UseMiddleware<JwtMiddleware>();

            app.MapControllers();
            ControllerActionEndpointConventionBuilder controllerActionEndpointConventionBuilder = app.MapControllers();

            app.Run();
        }
    }
}