using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; } = true;
        public int? DisplayOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public List<string>? Tags { get; set; }
        public string? Slug { get; set; }
        public bool? ShowOnHome { get; set; } = false;
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class CategoryDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; } = true;
        public int? DisplayOrder { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public List<string>? Tags { get; set; }
        public string? Slug { get; set; }
        public bool? ShowOnHome { get; set; } = false;
        public string? Icon { get; set; }
        public string? ColorCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
