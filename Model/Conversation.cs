using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Title { get; set; } // tên cuộc hội thoại nhóm hoặc null nếu là 1-1
        // người tạo
        //public string CreatorUserId { get; set; }

        public string? ConversationType { get; set; } // 1-1 hoặc nhóm

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
    public class ConversationDTO
    {
        public string? Id { get; set; }

        public string? Title { get; set; } // tên cuộc hội thoại nhóm hoặc null nếu là 1-1
        // người tạo
        //public string CreatorUserId { get; set; }

        public string? ConversationType { get; set; } // 1-1 hoặc nhóm

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }

}
