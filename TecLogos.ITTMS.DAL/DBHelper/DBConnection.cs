using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TecLogos.ITTMS.DAL.DBHelper
{
    public class DBConnection
    {
        private readonly string _connectionString;

        public DBConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
