using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Code { get; set; }
        public string? Position { get; set; }  // "Pha chế", "Thu ngân", ...
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; } = "Đang làm";  // "Đang làm", "Nghỉ việc"
        public string? Shift { get; set; }  // "Sáng", "Chiều", ...
        public string? IdentityNumber { get; set; }  // CMND/CCCD
        public string? AvatarUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }  // "Nam", "Nữ", "Khác"

        public string? Address { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public string? Role { get; set; }  // "Employee", "Customer"
        public string? MembershipLevel { get; set; } = "Thường";  // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public string? Note { get; set; }  // Ghi chú: "Khách VIP", ...
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

    public class UserDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Code { get; set; }
        public string? Position { get; set; }  // "Pha chế", "Thu ngân", ...
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; } = "Đang làm";  // "Đang làm", "Nghỉ việc"
        public string? Shift { get; set; }  // "Sáng", "Chiều", ...
        public string? IdentityNumber { get; set; }  // CMND/CCCD
        public string? AvatarUrl { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }  // "Nam", "Nữ", "Khác"

        public string? Address { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public string? Role { get; set; }  // "Employee", "Customer"
        public string? MembershipLevel { get; set; } = "Thường";  // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public string? Note { get; set; }  // Ghi chú: "Khách VIP", ...
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }

}
