using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class OrderCancellation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? OrderId { get; set; }
        public string? Reason { get; set; }
        public DateTime? CancelTime { get; set; }
        public string? EmployeeId { get; set; }   // người huyer đơn
    }
    public class OrderCancellationDTO
    {
        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? Reason { get; set; }
        public DateTime? CancelTime { get; set; }
        public string? EmployeeId { get; set; }   // người huyer đơn
    }
}
