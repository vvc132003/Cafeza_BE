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
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<User> _user;

        public class Employees
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Roles { get; set; }
        }
        public UserController(MongoDbContext context)
        {
            //_employee = context.Employees;
            _user = context.Users;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var users = await _user.Find(u => u.Role == "employee").ToListAsync();   
            return Ok(users);
        }


        [HttpPost]
        public ActionResult<User> Create([FromBody] UserDTO res)
        {
            var newUser = ToEntityUser(res);
            _user.InsertOne(newUser);
            return Ok(newUser);
        }


        [HttpPut("{id}")]
        public ActionResult<User> Update(string id, [FromBody] UserDTO dto)
        {
            var user = _user.Find(u => u.Id == id).FirstOrDefault();

            if (user == null)
            {
                return NotFound("User không tồn tại");
            }

            UpdateEntityUser(user, dto);

            _user.ReplaceOne(u => u.Id == id, user);

            return Ok(user);
        }
        private void UpdateEntityUser(User user, UserDTO dto)
        {
            user.FullName = dto.FullName ?? user.FullName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            user.Email = dto.Email ?? user.Email;

            if (!string.IsNullOrEmpty(dto.Password))
                user.Password = dto.Password;

            user.Code = dto.Code ?? user.Code;
            user.Position = dto.Position ?? user.Position;
            user.StartDate = dto.StartDate ?? user.StartDate;
            user.Salary = dto.Salary ?? user.Salary;
            user.Status = dto.Status ?? user.Status;
            user.Shift = dto.Shift ?? user.Shift;
            user.IdentityNumber = dto.IdentityNumber ?? user.IdentityNumber;
            user.AvatarUrl = dto.AvatarUrl ?? user.AvatarUrl;
            user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            user.Gender = dto.Gender ?? user.Gender;
            user.Address = dto.Address ?? user.Address;
            user.Role = dto.Role ?? user.Role;
            user.MembershipLevel = dto.MembershipLevel ?? user.MembershipLevel;

            if (dto.RewardPoints != null)
                user.RewardPoints = dto.RewardPoints;

            user.Note = dto.Note ?? user.Note;
            user.IsDeleted = dto.IsDeleted ?? user.IsDeleted;

            user.UpdatedAt = DateTime.Now;
        }

        private User ToEntityUser(UserDTO dto)
        {
            return new User
            {
                Id = dto.Id,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Password = dto.Password,
                Code = dto.Code,
                Position = dto.Position,
                StartDate = dto.StartDate,
                Salary = dto.Salary,
                Status = dto.Status,
                Shift = dto.Shift,
                IdentityNumber = dto.IdentityNumber,
                AvatarUrl = dto.AvatarUrl,
                UpdatedAt = dto.UpdatedAt,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                IsDeleted = dto.IsDeleted ?? false,
                Role = dto.Role,
                MembershipLevel = dto.MembershipLevel,
                RewardPoints = dto.RewardPoints ?? 0,
                Note = dto.Note,
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

            claims.Add(new Claim(ClaimTypes.Role, user.Role));


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
