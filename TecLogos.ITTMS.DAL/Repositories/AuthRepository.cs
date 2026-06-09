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
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            return await connection.QueryFirstOrDefaultAsync<Employee>(
                "sp_GetEmployeeByEmail",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<string?> GetPasswordHashByEmployeeIdAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_GetPasswordHashByEmployeeId",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task<string?> GetEmployeeRoleAsync(Guid employeeId)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);

            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_GetEmployeeRole",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <inheritdoc />
        public async Task LogLoginAsync(Guid employeeId, string ipAddress)
        {
            using var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeID", employeeId);
            parameters.Add("@IPAddress", ipAddress);

            await connection.ExecuteAsync(
                "sp_LogEmployeeLogin",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
