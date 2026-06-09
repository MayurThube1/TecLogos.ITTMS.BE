using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TecLogos.ITTMS.API.Middleware
{
    /// <summary>
    /// Custom middleware that validates JWT tokens from the Authorization header
    /// and attaches user claims to HttpContext.Items for downstream use.
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"]!;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Attach parsed claims to HttpContext for use in controllers
                context.Items["UserId"] = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                context.Items["UserEmail"] = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                context.Items["UserRole"] = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("JWT validation failed: {Message}", ex.Message);
                // Token is invalid — do nothing, the request will proceed unauthenticated.
                // The [Authorize] attribute on controllers will handle 401 responses.
            }
        }
    }
}
