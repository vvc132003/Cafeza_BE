using Cafeza_BE.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMongoCollection<Employee> _employee;
        private readonly IMongoCollection<Customer> _customer;
        public CustomerController(MongoDbContext context)
        {
            _employee = context.Employees;
            _customer = context.Customers;
        }
        //[HttpPost]
        //public ActionResult<Employee> Create([FromBody] EmployeeDTO newEmployeedto)
        //{
        //    var newEmployee = ToEntity(newEmployeedto);
        //    _employee.InsertOne(newEmployee);
        //    return Ok(newEmployee);
        //}

    }
}
