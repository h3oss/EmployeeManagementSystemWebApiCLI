using EmployeeManagement.API.Controllers;
using EmployeeManagementAPI;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EmployeeManagementAPI.Tests
{
    public class EmployeesControllerTests
    {
        private readonly EmployeeContext _context;
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            var options = new DbContextOptionsBuilder<EmployeeContext>()
                .UseInMemoryDatabase(databaseName: "EmployeeDatabase")
                .Options;

            _context = new EmployeeContext(options);
            _controller = new EmployeesController(_context);
        }

        [Fact]
        public async Task GetEmployees_ReturnsAllEmployees()
        {           
            var employee = new Employee { Name = "Влад", Position = "C# Developer", Salary = 100000, DateOfJoining = DateTime.Now };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetEmployees();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Employee>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task GetEmployeeByName_ValidName_ReturnsEmployee()
        {            
            var employee = new Employee {Name = "Андрей", Position = "Manager", Salary = 999999, DateOfJoining = DateTime.Now };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            
            var result = await _controller.GetEmployeeByName("Андрей");
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal("Андрей", returnValue.Name);
        }

        [Fact]
        public async Task PostEmployee_ValidEmployee_ReturnsCreatedEmployee()
        {            
            var employee = new Employee { Name = "Стас", Position = "Designer", Salary = 3000 };
            
            var result = await _controller.PostEmployee(employee);
            
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Employee>(createdAtActionResult.Value);
            Assert.Equal("Стас", returnValue.Name);
        }

        [Fact]
        public async Task DeleteEmployee_ValidId_ReturnsNoContent()
        {
            var employee = new Employee { Name = "Петр", Position = "Developer", Salary = 30000, DateOfJoining = DateTime.Now };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            
            var result = await _controller.DeleteEmployee("Петр");
            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CalculateSalary_ValidRequest_ReturnsTotalSalary()
        {            
            var employee = new Employee { Name = "Игорь", Position = "Manager", Salary = 6000, DateOfJoining = new DateTime(2023,1, 1) };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            
            var result = await _controller.CalculateSalary("Игорь", new DateTime(2023, 1, 1), new DateTime(2023, 1, 5));
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var totalSalary = Assert.IsType<decimal>(okResult.Value);
            Assert.Equal(1000, totalSalary); // 6000 / 30 * 5
        }
    }
}

