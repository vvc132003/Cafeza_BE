using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? ConversationId { get; set; }
        public string? SenderMemberId { get; set; }
        public string? MessageType { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? ParentId { get; set; }


    }
    public class MessageDTO
    {
        public string? Id { get; set; }
        public string? ConversationId { get; set; }
        public string? SenderMemberId { get; set; }
        public string? MessageType { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? ParentId { get; set; }


    }
}
