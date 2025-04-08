using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [BsonElement("CustomerId")]
        public string? CustomerId { get; set; }   // Có thể null nếu khách lẻ
        [BsonElement("EmployeeId")]
        public string? EmployeeId { get; set; }   // Người tạo đơn
        [BsonElement("TableId")]
        public string? TableId { get; set; }      // Bàn nào (nếu tại quán)
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt"; // "Tiền mặt", "Chuyển khoản", "Momo"
        public string Status { get; set; } = "Chờ thanh toán"; // "Đã thanh toán", "Đã huỷ", "Đang xử lý"
        public string? Note { get; set; } // Ghi chú thêm
    }

}
