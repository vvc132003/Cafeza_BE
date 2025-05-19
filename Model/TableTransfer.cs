using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Model
{
    public class TableTransfer
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string? FromTableId { get; set; }
        public string? ToTableId { get; set; }
        public DateTime? TransferTime { get; set; }
        public string? EmployeeId { get; set; }   // người chuyển
        public string? Note { get; set; }
    }
    public class TableTransferDTO
    {

        public string? Id { get; set; }
        public string? OrderId { get; set; }
        public string? FromTableId { get; set; }
        public string? ToTableId { get; set; }
        public DateTime? TransferTime { get; set; }
        public string? EmployeeId { get; set; }   // người chuyển
        public string? Note { get; set; }
    }
}
