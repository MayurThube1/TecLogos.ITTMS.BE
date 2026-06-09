using System.Collections.Generic;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.Entities;

namespace TecLogos.ITTMS.DAL.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public IEnumerable<Employee> GetAll()
        {
            // TODO: implement DB access
            return new List<Employee>();
        }

        public Employee? GetById(int id)
        {
            // TODO: implement DB access
            return null;
        }
    }
}
