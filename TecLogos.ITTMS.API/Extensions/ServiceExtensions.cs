using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.BLL.Services;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.DAL.Repositories;

namespace TecLogos.ITTMS.API.Extensions
{
    /// <summary>
    /// Extension methods for registering application services, JWT authentication, and CORS.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers application-specific services (DBConnection, Repositories, Services) into the DI container.
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Database connection helper (singleton — holds only the connection string)
            services.AddSingleton<DBConnection>();

            // Repositories (scoped — one instance per HTTP request)
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IEmployeeDashboardRepository, EmployeeDashboardRepository>();
            services.AddScoped<IITSupportEngineerRepository, ITSupportEngineerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            // Business logic services (scoped)
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeDashboardService, EmployeeDashboardService>();
            services.AddScoped<IITSupportEngineerService, ITSupportEngineerService>();
            services.AddScoped<IEmployeeService, EmployeeService>();

            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication from appsettings.json "JwtSettings" section.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json.");

            var key = Encoding.UTF8.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Allow HTTP in development
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            return services;
        }

        /// <summary>
        /// Configures CORS to allow the React frontend to communicate with this API.
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",   // Vite dev server
                            "http://localhost:3000",   // Alternative dev server
                            "http://localhost:5174"    // Vite secondary port
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}