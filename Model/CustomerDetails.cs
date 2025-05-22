using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class CustomerDetails
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? UserId { get; set; }
        public string? MembershipLevel { get; set; } = "Thường";  // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public string? Note { get; set; }  // Ghi chú: "Khách VIP", ...
    }

    public class CustomerDetailsDTO
    {      
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? MembershipLevel { get; set; } = "Thường";  // Thường, Bạc, Vàng, Kim cương
        public int? RewardPoints { get; set; } = 0;
        public string? Note { get; set; }  // Ghi chú: "Khách VIP", ...
    }

}
