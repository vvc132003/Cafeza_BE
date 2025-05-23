using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? Code { get; set; }
        public string? CustomerId { get; set; }   // Có thể null nếu khách lẻ
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; } // status = 10 thì giỏ hàng còn sử dụng or = 90 thì giỏ hàng đã xử lý xong(mua)
        public string? Note { get; set; } // Ghi chú thêm

    }
    public class CartDTO
    {
      
        public string? Id { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? Code { get; set; }
        public string? CustomerId { get; set; }   // Có thể null nếu khách lẻ
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; } // status = 10 thì giỏ hàng còn sử dụng or = 90 thì giỏ hàng đã xử lý xong(mua)
        public string? Note { get; set; } // Ghi chú thêm

    }
}
