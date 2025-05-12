using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cafeza_BE.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IMongoCollection<Employee> _employee;
        private readonly List<Employees> _employees;
        public class Employees
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Roles { get; set; }
        }
        public EmployeeController(MongoDbContext context)
        {
            _employee = context.Employees;
            _employees = new List<Employees>
            {
                new Employees { Email = "admin", Password = "admin123", Roles= "Admin" },
                new Employees { Email = "staff@example.com", Password = "staff123", Roles = "Staff" }
            };
        }

        [HttpPost]
        public ActionResult<Employee> Create([FromBody] EmployeeDTO newEmployeedto)
        {
            var newEmployee = ToEntity(newEmployeedto);
            _employee.InsertOne(newEmployee);
            return Ok(newEmployee);
        }

        private Employee ToEntity(EmployeeDTO dto)
        {
            return new Employee
            {
                Code = dto.Code,
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Password = dto.Password,
                Position = dto.Position,
                StartDate = dto.StartDate,
                Salary = dto.Salary,
                Status = dto.Status,
                Address = dto.Address,
                Shift = dto.Shift,
                IdentityNumber = dto.IdentityNumber,
                AvatarUrl = dto.AvatarUrl,
                Roles = dto.Roles ?? new List<string>(),
                IsDeleted = dto.IsDeleted,
                CreatedAt = dto.CreatedAt ?? DateTime.Now
            };
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel login)
        {
            var user = Authenticate(login.Email, login.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            string token = GenerateJwtToken(user);
            return Ok(new { Token = token });
            //return Ok(new { message = "Login successful" });

        }
        public class UserLoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        private Employees Authenticate(string email, string password)
        {
            var employee = _employees.FirstOrDefault(user => user.Email == email && user.Password == password);
            //var employee = _employee.Find(user => user.Email == email && user.Password == password).FirstOrDefault();
            return employee;
        }

        private string GenerateJwtToken(Employees employee)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, employee.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", employee.Email),
            new Claim(ClaimTypes.Role, employee.Roles)
    };

            //// thêm từng quyền (role) làm claim
            //foreach (var role in employee.Roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("[}61L3B>z?XvzH&#!jH?b_RJ=K£lh-J7TO~c+i"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "cafeza",
                audience: "api-cafeza",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);
            //var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //// Lưu vào Cookie HttpOnly
            //HttpContext.Response.Cookies.Append("access_token", tokenString, new CookieOptions
            //{
            //    //HttpOnly = true,
            //    //Secure = false,
            //    SameSite = SameSiteMode.Strict,
            //    Expires = DateTime.UtcNow.AddHours(1),
            //    Path = "/"
            //});
            return new JwtSecurityTokenHandler().WriteToken(token);
            //return tokenString;
        }


    }
}
