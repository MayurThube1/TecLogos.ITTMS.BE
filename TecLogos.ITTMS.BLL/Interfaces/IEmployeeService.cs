using TecLogos.ITTMS.Models.DTOs;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDTO> GetAll();
        EmployeeDTO? GetById(Guid id);
    }
}
