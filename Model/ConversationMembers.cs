using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class ConversationMembers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? ConversationId { get; set; }
        public string? MemberId { get; set; } // người tham gia
        public string? MemberType { get; set; } // "employee" hoặc "customer"
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        //public string Recipient { get; set; }
    }
    public class ConversationMembersDTO
    {
        public string? Id { get; set; }
        public string? ConversationId { get; set; }
        public string? MemberId { get; set; } // người tham gia
        public string? MemberType { get; set; } // "employee" hoặc "customer"
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        //public string Recipient { get; set; }
    }
}
