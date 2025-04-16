using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Customer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; } // "Nam", "Nữ", "Khác"
        public string? Address { get; set; }
        public string? MembershipLevel { get; set; } = "Thường"; // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool? IsDeleted { get; set; } = false;
        public string? Note { get; set; }   // Ghi chú: "Khách VIP", "Hay complain" 😅

    }
    public class CustomerDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; } // "Nam", "Nữ", "Khác"
        public string? Address { get; set; }
        public string? MembershipLevel { get; set; } = "Thường"; // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool? IsDeleted { get; set; } = false;
        public string? Note { get; set; }   // Ghi chú: "Khách VIP", "Hay complain" 😅

    }

}
