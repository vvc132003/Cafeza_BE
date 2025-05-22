using System.Data;
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
        private readonly IMongoCollection<EmployeeDetails> _employeedetails;
        private readonly IMongoCollection<User> _user;
        private readonly List<Employees> _employees;
        public class Employees
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Roles { get; set; }
        }
        public EmployeeController(MongoDbContext context)
        {
            //_employee = context.Employees;
            _employeedetails = context.EmployeeDetails;
            _user = context.Users;
            _employees = new List<Employees>
            {
                new Employees { Email = "admin", Password = "admin123", Roles= "Admin" },
                new Employees { Email = "staff@example.com", Password = "staff123", Roles = "Staff" }
            };
        }

        public class EmployeeRes
        {
            public EmployeeDetailsDTO EmployeeDetailsDTO { get; set; }
            public UserDTO UserDTO { get; set; }
        }

        [HttpPost]
        public ActionResult<Employee> Create([FromBody] EmployeeRes res)
        {
            var newUser = ToEntityUser(res.UserDTO);
            _user.InsertOne(newUser);
            res.EmployeeDetailsDTO.UserId = newUser.Id;
            var newEmployee = ToEntityEmployee(res.EmployeeDetailsDTO);
            _employeedetails.InsertOne(newEmployee);
            return Ok(newEmployee);
        }

        private EmployeeDetails ToEntityEmployee(EmployeeDetailsDTO dto)
        {
            return new EmployeeDetails
            {
                UserId = dto.UserId,
                Code = dto.Code,
                Position = dto.Position,
                StartDate = dto.StartDate,
                Salary = dto.Salary,
                Status = dto.Status,
                Shift = dto.Shift,
                IdentityNumber = dto.IdentityNumber,
                AvatarUrl = dto.AvatarUrl,
                Roles = dto.Roles ?? new List<string>(),
            };
        }

        private User ToEntityUser(UserDTO dto)
        {
            return new User
            {
               FullName = dto.FullName,
               PhoneNumber = dto.PhoneNumber,
               Email = dto.Email,
               Password = dto.Password, 
               DateOfBirth = dto.DateOfBirth,
               Gender = dto.Gender,
               Address = dto.Address,
                IsDeleted = dto.IsDeleted,
                Role = dto.Role,
                CreatedAt = dto.CreatedAt ?? DateTime.Now
            };
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginModel login)
        {
            var employee = Authenticate(login.Email, login.Password);

            if (employee == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
            string token = GenerateJwtToken(employee);
            return Ok(new { Token = token });
            //return Ok(new { message = "Login successful" });

        }
        public class UserLoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
        private User Authenticate(string email, string password)
        {
            //var employee = _employees.FirstOrDefault(user => user.Email == email && user.Password == password);
            var user = _user.Find(user => user.Email == email && user.Password == password).FirstOrDefault();
            return user;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", user.Id),
            //new Claim(ClaimTypes.Role, employee.Roles)
    };
            if(user.Role == "employee")
            {
                var employee = _employeedetails.Find(e => e.UserId == user.Id).FirstOrDefault();
            // thêm từng quyền (role) làm claim
                foreach (var role in employee.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role));
            }


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
