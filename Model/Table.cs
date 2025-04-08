using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Table
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? TableName { get; set; }
        public int? Capacity { get; set; } // số lượng người ngồi
        public string? Status { get; set; } = "Available";
        public string? Type { get; set; }
        public string? Location { get; set; } /// khu vực
        public string? Note { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool IsReservable { get; set; } = true; // cho phép đặt chổ online
        public List<string>? Tags { get; set; }
        public TimeSpan? ReservedFrom { get; set; }  // Giờ bắt đầu giữ bàn
        public TimeSpan? ReservedTo { get; set; }   // Giờ kết thúc giữ bàn
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

    }
    public class TableDTO
    {
        public string? Id { get; set; }
        public string? TableName { get; set; }
        public int? Capacity { get; set; } // số lượng người ngồi
        public string? Status { get; set; } = "Available";
        public string? Type { get; set; }
        public string? Location { get; set; } /// khu vực
        public string? Note { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool IsReservable { get; set; } = true; // cho phép đặt chổ online
        public List<string>? Tags { get; set; }
        public TimeSpan? ReservedFrom { get; set; }  // Giờ bắt đầu giữ bàn
        public TimeSpan? ReservedTo { get; set; }   // Giờ kết thúc giữ bàn
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

    }

}
