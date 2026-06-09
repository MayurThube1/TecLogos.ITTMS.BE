using System.Collections.Generic;
using TecLogos.ITTMS.Models.DTOs;

namespace TecLogos.ITTMS.BLL.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDTO> GetAll();
        EmployeeDTO? GetById(int id);
    }
}
