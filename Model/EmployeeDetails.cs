using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class EmployeeDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? UserId { get; set; }
        public string? Code { get; set; }
        public string? Position { get; set; }  // "Pha chế", "Thu ngân", ...
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; } = "Đang làm";  // "Đang làm", "Nghỉ việc"
        public string? Shift { get; set; }  // "Sáng", "Chiều", ...
        public string? IdentityNumber { get; set; }  // CMND/CCCD
        public string? AvatarUrl { get; set; }
        public List<string>? Roles { get; set; }  // Quyền: ["Order", "Quản lý", ...]
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmployeeDetailsDTO
    {
       
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? Code { get; set; }
        public string? Position { get; set; }  // "Pha chế", "Thu ngân", ...
        public DateTime? StartDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; } = "Đang làm";  // "Đang làm", "Nghỉ việc"
        public string? Shift { get; set; }  // "Sáng", "Chiều", ...
        public string? IdentityNumber { get; set; }  // CMND/CCCD
        public string? AvatarUrl { get; set; }
        public List<string>? Roles { get; set; }  // Quyền: ["Order", "Quản lý", ...]
        public DateTime? UpdatedAt { get; set; }
    }

}
