using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; } // "Nam", "Nữ", "Khác"
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Position { get; set; } // "Pha chế", "Thu ngân", "Phục vụ", "Quản lý"
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string Status { get; set; } = "Đang làm"; // "Đang làm", "Nghỉ việc"
        public string? Address { get; set; }
        public string? Shift { get; set; }   // Ca làm: "Sáng", "Chiều", "Tối", "Full"
        public string? IdentityNumber { get; set; } // CMND/CCCD
        public string? AvatarUrl { get; set; }      // Ảnh đại diện
        public List<string> Roles { get; set; }    // Quyền: ["Order", "Quản lý", "Thống kê"]
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

    }
    public class EmployeeDTO
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; } // "Nam", "Nữ", "Khác"
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password{ get; set; }
        public string? Position { get; set; } // "Pha chế", "Thu ngân", "Phục vụ", "Quản lý"
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string Status { get; set; } = "Đang làm"; // "Đang làm", "Nghỉ việc"
        public string? Address { get; set; }
        public string? Shift { get; set; }   // Ca làm: "Sáng", "Chiều", "Tối", "Full"
        public string? IdentityNumber { get; set; } // CMND/CCCD
        public string? AvatarUrl { get; set; }      // Ảnh đại diện
        public List<string> Roles { get; set; }    // Quyền: ["Order", "Quản lý", "Thống kê"]
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

    }

}
