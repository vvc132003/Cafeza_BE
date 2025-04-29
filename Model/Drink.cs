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
        public string Sku { get; set; }
        public string? Slug { get; set; }
        public string Name { get; set; }
        public string? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } // "available", "out_of_stock", "discontinued"
        public string Size { get; set; } // "small", "medium", "large"
        public int? ViewCount { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; } 
    }
    public class DrinkDTO
    {
        public string? Id { get; set; }
        public string Sku { get; set; }
        public string? Slug { get; set; }
        public string Name { get; set; }
        public string? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }  // "available", "out_of_stock", "discontinued"
        public string Size { get; set; }    // "small", "medium", "large"
        public int? ViewCount { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
