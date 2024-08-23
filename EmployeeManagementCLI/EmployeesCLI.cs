using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime DateOfJoining { get; set; }
    public decimal Salary { get; set; }
    public string Position { get; set; }
}

public class EmployeesCLI
{
    private static readonly HttpClient client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7216/") 
    };


    public static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("==================================");
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Добавить сотрудника");
            Console.WriteLine("2. Получить список сотрудников");
            Console.WriteLine("3. Поиск сотрудника по имени");
            Console.WriteLine("4. Обновить данные сотрудника");
            Console.WriteLine("5. Удалить сотрудника");
            Console.WriteLine("6. Рассчет заработной платы");
            Console.WriteLine("0. Выход");
            Console.WriteLine("==================================");
            Console.WriteLine("Введите номер операции");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": await AddEmployee(); break;
                case "2": await GetEmployees(); break;
                case "3": await SearchEmployeeByName(); break;
                case "4": await UpdateEmployee(); break;
                case "5": await DeleteEmployee(); break;
                case "6": await CalculateSalary(); break;
                case "0": return;
                default: Console.WriteLine("Некорректный выбор. Пожалуйста, попробуйте снова."); break;
            }
        }
    }

    public static async Task GetEmployees()
    {
        var employees = await client.GetFromJsonAsync<List<Employee>>("api/Employees");
        foreach (var employee in employees)
            PrintEmployee(employee);
    }

    public static async Task AddEmployee()
    {
        var employee = await GetEmployeeDetails();
        var response = await client.PostAsJsonAsync("api/Employees", employee);
        HandleResponse(response, "Сотрудник добавлен.");
    }

    public static async Task SearchEmployeeByName()
    {        
        string name = GetValidString("Введите имя для поиска: ", "Имя не может быть пустым.", @"^[a-zA-ZА-Яа-яЁё\s]+$");
        
        var employees = await client.GetFromJsonAsync<List<Employee>>("api/Employees");
        
        var filteredEmployees = employees.Where(e => e.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        
        if (filteredEmployees.Any())
        {
            Console.WriteLine($"Найдено {filteredEmployees.Count} сотрудника(ов) с именем, содержащим \"{name}\":");
            foreach (var employee in filteredEmployees)
            {
                PrintEmployee(employee);
            }
        }
        else
        {
            Console.WriteLine($"Сотрудники с именем, содержащим \"{name}\", не найдены.");
        }
    }
    public static async Task UpdateEmployee()
    {
        int id = GetValidInt("Введите ID сотрудника: ", "Некорректный ID.");
        var employee = await GetEmployeeDetails();
        employee.Id = id;
        var response = await client.PutAsJsonAsync($"api/Employees/{id}", employee);
        HandleResponse(response, "Данные сотрудника обновлены.");
    }

    public static async Task DeleteEmployee()
    {
        int id = GetValidInt("Введите ID сотрудника: ", "Некорректный ID.");
        var response = await client.DeleteAsync($"api/Employees/{id}");
        HandleResponse(response, "Сотрудник удален.");
    }

    public static async Task CalculateSalary()
    {
        string name;
        bool employeeExists = false;
        var namePattern = @"^[a-zA-ZА-Яа-яЁё\s]+$";

        do
        {
            Console.Write("Введите имя сотрудника: ");
            name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Имя сотрудника не может быть пустым. Попробуйте снова.");
                continue;
            }

            if (!Regex.IsMatch(name, namePattern))
            {
                Console.WriteLine("Имя может содержать только буквы. Попробуйте снова.");
                continue;
            }

            var response = await client.GetAsync($"api/Employees/name/{Uri.EscapeDataString(name)}");
            employeeExists = response.IsSuccessStatusCode;

            if (!employeeExists)
            {
                Console.WriteLine($"Сотрудник с именем '{name}' не найден. Пожалуйста, введите имя снова.");
            }

        } while (string.IsNullOrWhiteSpace(name) || !employeeExists);

        DateTime startDate = GetValidDate("Введите начальную дату (yyyy-MM-dd): ");
        DateTime endDate = GetEndDate(startDate);
        
        var salaryResponse = await client.GetFromJsonAsync<decimal>(
            $"api/employees/salary?name={Uri.EscapeDataString(name)}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        Console.WriteLine($"Общая зарплата сотрудника {name} с {startDate:yyyy-MM-dd} по {endDate:yyyy-MM-dd}: {salaryResponse:C}");
    }
    private static async Task<Employee> GetEmployeeDetails()
    {
        string name = GetValidString("Введите имя: ", "Имя не может быть пустым.", @"^[a-zA-ZА-Яа-яЁё\s]+$");
        int age = GetValidInt("Введите возраст: ", "Возраст должен быть положительным числом.");
        string position = GetValidString("Введите должность: ", "Должность не может быть пустой.", @"^[a-zA-ZА-Яа-яЁё\s]+$");
        decimal salary = GetValidDecimal("Введите зарплату: ", "Зарплата должна быть положительной.");
        DateTime dateOfJoining = GetValidDate("Введите начальную дату (yyyy-MM-dd): ");

        return new Employee { Name = name, Age = age, Position = position, Salary = salary, DateOfJoining = dateOfJoining };
    }

    private static string GetValidString(string prompt, string errorMessage, string pattern)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, pattern))
                return input;
            Console.WriteLine(errorMessage);
        }
    }

    private static int GetValidInt(string prompt, string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                return value;
            Console.WriteLine(errorMessage);
        }
    }

    private static decimal GetValidDecimal(string prompt, string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
                return value;
            Console.WriteLine(errorMessage);
        }
    }

    private static DateTime GetValidDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                return date;
            Console.WriteLine("Некорректный формат даты. Попробуйте снова.");
        }
    }

    private static DateTime GetEndDate(DateTime startDate)
    {
        while (true)
        {
            DateTime endDate = GetValidDate("Введите конечную дату (yyyy-MM-dd): ");
            if (endDate >= startDate)
                return endDate;
            Console.WriteLine("Конечная дата должна быть больше или равной начальной дате.");
        }
    }

    private static void PrintEmployee(Employee employee)
    {
        Console.WriteLine($"{employee.Id}, {employee.Name}, {employee.Age}, {employee.Position}, {employee.Salary}, {employee.DateOfJoining}");
    }

    private static void HandleResponse(HttpResponseMessage response, string successMessage)
    {
        if (response.IsSuccessStatusCode)
            Console.WriteLine(successMessage);
        else
            Console.WriteLine($"Ошибка: {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}");
    }
}