namespace TecLogos.ITTMS.Models.DTOs
{
    /// <summary>
    /// Response payload returned after successful authentication.
    /// </summary>
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public UserInfoDTO User { get; set; } = null!;
    }

    /// <summary>
    /// Minimal user info returned to the frontend after login.
    /// Matches the shape expected by the React authService.
    /// </summary>
    public class UserInfoDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
