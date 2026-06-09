using TecLogos.ITTMS.Models.Entities;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    /// <summary>
    /// Data access contract for authentication operations.
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Retrieves an employee by email address.
        /// Returns null if no active, non-deleted employee is found.
        /// </summary>
        Task<Employee?> GetEmployeeByEmailAsync(string email);

        /// <summary>
        /// Retrieves the password hash from the Authentication table for a given employee.
        /// Returns null if no active authentication record is found.
        /// </summary>
        Task<string?> GetPasswordHashByEmployeeIdAsync(Guid employeeId);

        /// <summary>
        /// Retrieves the role name for a given employee from the EmployeeRoles → Role join.
        /// Returns null if no active role assignment is found.
        /// </summary>
        Task<string?> GetEmployeeRoleAsync(Guid employeeId);

        /// <summary>
        /// Logs a successful login by updating/inserting into the Authentication table.
        /// </summary>
        Task LogLoginAsync(Guid employeeId, string ipAddress);
    }
}
