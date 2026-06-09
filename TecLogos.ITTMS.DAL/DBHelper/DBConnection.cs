using System;
using System.Collections.Generic;
using System.Text;

namespace TecLogos.ITTMS.DAL.DBHelper
{
    public class DBConnection
    {
        private readonly string _connectionString;

        public DBConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
