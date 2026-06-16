using Microsoft.OpenApi.Models;
using System.Reflection;
using TecLogos.ITTMS.API.Extensions;
using TecLogos.ITTMS.API.Middleware;
using TecLogos.ITTMS.Common.Constants;

namespace TecLogos.ITTMS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Configure Swagger/Swashbuckle
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TecLogos ITTMS API",
                    Version = "v1",
                    Description = "IT Ticket & Asset Management System API"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Resolve schema ID conflicts by using full type names
                c.CustomSchemaIds(type => type.FullName);
            });

            // Register application services (DI for repos, services, DB)
            builder.Services.AddApplicationServices();

            // Configure JWT authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Configure CORS for React frontend
            builder.Services.AddCorsPolicy();

            // Load app constants dynamically from configuration
            AppConstants.LoadFromConfiguration(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ITTMS API v1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.DefaultModelsExpandDepth(-1);
                });
            }

            // Global exception handler (must be first in pipeline)
            app.UseMiddleware<ExceptionMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // CORS must come before auth
            app.UseCors("AllowFrontend");

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom JWT middleware (attaches user claims to HttpContext.Items)
            app.UseMiddleware<JwtMiddleware>();

            app.MapControllers();

            app.Run();
        }
    }
}