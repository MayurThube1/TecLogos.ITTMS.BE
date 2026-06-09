using System.Collections.Generic;
using TecLogos.ITTMS.BLL.Interfaces;
using TecLogos.ITTMS.Models.DTOs;
using TecLogos.ITTMS.Models.Entities;
using TecLogos.ITTMS.DAL.Interfaces;

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
                result.Add(new EmployeeDTO { Id = e.Id, Name = e.Name });
            }
            return result;
        }

        public EmployeeDTO? GetById(int id)
        {
            var e = _repo.GetById(id);
            if (e == null) return null;
            return new EmployeeDTO { Id = e.Id, Name = e.Name };
        }
    }
}