using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EmployeeManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeContext _context;

        public EmployeesController(EmployeeContext context)
        {
            _context = context;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var employees = await _context.Employees.ToListAsync();
            return Ok(employees);
        }

        // GET: api/employees/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Employee>> GetEmployeeByName(string name)
        {            
            if (!IsValidName(name))
            {
                return BadRequest("Имя может содержать только буквы и пробелы.");
            }
            
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());

            if (employee == null)
            {
                return NotFound($"Сотрудник с именем '{name}' не найден.");
            }
            
            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            var validationResult = ValidateEmployee(employee);
            if (validationResult != null)
            {
                return validationResult;
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmployeeByName), new { name = employee.Name }, employee);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest("ID не совпадает.");
            }

            var validationResult = ValidateEmployee(employee);
            if (validationResult != null)
            {
                return validationResult;
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        // DELETE: api/employees/{name}
        [HttpDelete("name/{name}")] 
        public async Task<IActionResult> DeleteEmployee(string name)
        {            
            if (!IsValidName(name))
            {
                return BadRequest("Имя может содержать только буквы");
            }
           
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());

            if (employee == null)
            {
                return NotFound($"Сотрудник с именем '{name}' не найден.");
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync(); 
            return NoContent(); 
        }

        // GET: api/employees/salary
        [HttpGet("salary")]
        public async Task<ActionResult<decimal>> CalculateSalary(string name, DateTime startDate, DateTime endDate)
        {            
            if (!IsValidName(name))
            {
                return BadRequest("Имя может содержать только буквы и пробелы.");
            }
            
            if (endDate < startDate)
            {
                return BadRequest("Конечная дата должна быть больше или равна начальной дате.");
            }
            
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());

            if (employee == null)
            {
                return NotFound($"Сотрудник с именем '{name}' не найден.");
            }

            // Подсчет фактического количества рабочих дней
            int totalDaysWorked = (endDate - startDate).Days + 1;
            if (totalDaysWorked <= 0)
            {
                return BadRequest("Период работы должен содержать хотя бы один день.");
            }

            decimal dailySalary = employee.Salary / 30; // 30 - среднее количество дней в месяце
            decimal totalSalary = dailySalary * totalDaysWorked;

            return Ok(totalSalary);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        private ActionResult ValidateEmployee(Employee employee)
        {            
            if (string.IsNullOrWhiteSpace(employee.Name))
            {
                return BadRequest("Имя сотрудника не может быть пустым.");
            }
            if (!IsValidName(employee.Name))
            {
                return BadRequest("Имя может содержать только буквы");
            }
            if (string.IsNullOrWhiteSpace(employee.Position))
            {
                return BadRequest("Должность сотрудника не может быть пустым.");
            }
            if (!IsValidName(employee.Position))
            {
                return BadRequest("Должность может содержать только буквы");
            }            
            if (employee.Salary <= 0)
            {
                return BadRequest("Зарплата должна быть положительным значением.");
            }

            return null;
        }

        //Проверка слов 
        private bool IsValidName(string word)
        {
            string namePattern = @"^[a-zA-ZА-Яа-яЁё\s]+$";
            return Regex.IsMatch(word, namePattern);
        }
    }
}