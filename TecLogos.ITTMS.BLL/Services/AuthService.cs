using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;

namespace TecLogos.ITTMS.BLL.Services
{
    /// <summary>
    /// Handles authentication logic: credential verification, JWT generation, and login logging.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, string ipAddress)
        {
            _logger.LogInformation("Authentication process started for email: {Email}", request.Email);

            // 1. Find employee by email
            var employee = await _authRepository.GetEmployeeByEmailAsync(request.Email);
            if (employee == null)
            {
                _logger.LogWarning("Authentication failed: Employee record not found for email: {Email}", request.Email);
                return null;
            }

            _logger.LogInformation("Employee record found. ID: {ID}, IsActive: {IsActive}, IsDeleted: {IsDeleted}", employee.ID, employee.IsActive, employee.IsDeleted);

            // 2. Check if the employee is active and not deleted
            if (!employee.IsActive || employee.IsDeleted)
            {
                _logger.LogWarning("Authentication failed: Employee is either inactive or deleted. Active: {IsActive}, Deleted: {IsDeleted}", employee.IsActive, employee.IsDeleted);
                return null;
            }

            // 3. Get password hash from the Authentication table
            var storedHash = await _authRepository.GetPasswordHashByEmployeeIdAsync(employee.ID);
            if (string.IsNullOrEmpty(storedHash))
            {
                _logger.LogWarning("Authentication failed: No active password hash found in the Authentication table for EmployeeID: {EmployeeID}", employee.ID);
                return null;
            }

            _logger.LogInformation("Password hash retrieved from database. Verifying with BCrypt...");

            // 4. Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
            {
                _logger.LogWarning("Authentication failed: Password hash mismatch for email: {Email}", request.Email);
                return null;
            }

            _logger.LogInformation("Password successfully verified. Fetching role...");

            // 5. Get the employee's role
            var roleName = await _authRepository.GetEmployeeRoleAsync(employee.ID);
            if (string.IsNullOrEmpty(roleName))
            {
                _logger.LogInformation("No explicit role assigned. Defaulting to 'Employee' role.");
                roleName = "Employee"; // Default role if none assigned
            }
            else
            {
                _logger.LogInformation("Retrieved role for employee: {Role}", roleName);
            }

            // 6. Generate JWT token
            var token = GenerateJwtToken(employee.ID, employee.Email!, employee.FullName, roleName);

            // 7. Log the successful login
            await _authRepository.LogLoginAsync(employee.ID, ipAddress);

            _logger.LogInformation("Authentication successful. JWT generated for employee: {Email}", request.Email);

            // 8. Return response
            return new LoginResponseDTO
            {
                Token = token,
                User = new UserInfoDTO
                {
                    Id = employee.ID,
                    Email = employee.Email!,
                    Name = employee.FullName,
                    Role = roleName
                }
            };
        }

        /// <summary>
        /// Generates a signed JWT token with employee claims.
        /// </summary>
        private string GenerateJwtToken(Guid employeeId, string email, string fullName, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
            var issuer = jwtSettings["Issuer"] ?? "TeclogosITTMS";
            var audience = jwtSettings["Audience"] ?? "TeclogosITTMSUsers";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, employeeId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Name, fullName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
