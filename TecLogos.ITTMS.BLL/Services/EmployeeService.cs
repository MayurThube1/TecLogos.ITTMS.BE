using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.DAL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;

namespace TecLogos.ITTMS.BLL.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<EmployeeDTO> GetAll()
        {
            var items = _repo.GetAll();
            var result = new List<EmployeeDTO>();
            foreach (var e in items)
            {
                result.Add(new EmployeeDTO { Id = e.ID, Name = e.FullName });
            }
            return result;
        }

        public EmployeeDTO? GetById(Guid id)
        {
            var e = _repo.GetById(id);
            if (e == null) return null;
            return new EmployeeDTO { Id = e.ID, Name = e.FullName };
        }
    }
}
