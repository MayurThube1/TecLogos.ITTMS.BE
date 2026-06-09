using TecLogos.ITTMS.Models.DTOs;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    /// <summary>
    /// Business logic contract for authentication operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates an employee with email and password.
        /// Returns a LoginResponseDTO with JWT token and user info on success, or null on failure.
        /// </summary>
        Task<LoginResponseDTO?> AuthenticateAsync(LoginRequestDTO request, string ipAddress);
    }
}
