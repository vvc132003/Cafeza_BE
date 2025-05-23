using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class CartDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? CartId { get; set; }
        public string DrinkId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total;
        public string? Note { get; set; }
        public string? Status { get; set; } /// = 10 thì chọn mua or = 90 ko mua
    }

    public class CartDetailDTO
    {
       
        public string? Id { get; set; }
        public string? CartId { get; set; }
        public string DrinkId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total;
        public string? Note { get; set; }
        public string? Status { get; set; } /// = 10 thì chọn mua or = 90 ko mua
    }
}
