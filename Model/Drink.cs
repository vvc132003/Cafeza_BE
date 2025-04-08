using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Drink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("sku")]
        public string Sku { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("categoryId")]
        public string? CategoryId { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("imagePath")]
        public string? ImagePath { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // "available", "out_of_stock", "discontinued"

        [BsonElement("size")]
        public string Size { get; set; } // "small", "medium", "large"

        [BsonElement("createdAt")]
        public DateTime? CreatedAt { get; set; } 

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; } 
    }
    public class DrinkDTO
    {
        public string? Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }  // "available", "out_of_stock", "discontinued"
        public string Size { get; set; }    // "small", "medium", "large"
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
