using Dapper;
using System.Data;
using TecLogos.ITTMS.DAL.DBHelper;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.Entities;

namespace TecLogos.ITTMS.DAL.Repositories
{
    /// <summary>
    /// Dapper-based repository for authentication data access using stored procedures.
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly DBConnection _dbConnection;

        public AuthRepository(DBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <inheritdoc />
        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<Employee>(
                "sp_GetEmployeeByEmail",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<string?> GetPasswordHashByEmployeeIdAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_GetPasswordHashByEmployeeId",
                new { EmployeeID = employeeId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<string?> GetEmployeeRoleAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_GetEmployeeRole",
                new { EmployeeID = employeeId },
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task LogLoginAsync(Guid employeeId, string ipAddress)
        {
            using var connection = _dbConnection.GetConnection();

            await connection.ExecuteAsync(
                "sp_LogEmployeeLogin",
                new { EmployeeID = employeeId, IPAddress = ipAddress },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
