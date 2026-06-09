using TecLogos.ITTMS.Models.Entities;

namespace TecLogos.ITTMS.DAL.Interfaces
{
    public interface IEmployeeRepository
    {
        IEnumerable<Employee> GetAll();
        Employee? GetById(Guid id);
    }
}
