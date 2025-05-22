using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly IMongoCollection<Table> _table;
        private readonly IMongoCollection<Order> _order;
        private readonly IMongoCollection<OrderDetail> _orderDetail;
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<CustomerDetails> _customer;
        private readonly IMongoCollection<EmployeeDetails> _employee;
        private readonly IMongoCollection<User> _user;

        private readonly IHubContext<SignalRHub> _hubContext;

        public TableController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _table = context.Tables;
            _hubContext = hubContext;
            _order = context.Orders;
            _customer = context.CustomerDetails;
            _employee = context.EmployeeDetails;
            _orderDetail = context.OrderDetails;
            _drink = context.Drinks;
            _user = context.Users;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            var tables = await _table.Find(_ => true).ToListAsync();

            var data = new List<object>();

            foreach (var table in tables)
            {
                var order = await _order.Find(o => o.TableId == table.Id).FirstOrDefaultAsync();
                var orderDetails = order != null
                    ? await _orderDetail.Find(od => od.OrderId == order.Id).ToListAsync()
                    : new List<OrderDetail>();

                var totalQuantity = orderDetails.Sum(od => od.Quantity);
                var total = orderDetails.Sum(od => od.Total);

                var customer = order?.CustomerId != null
                    ? await _user.Find(c => c.Id == order.CustomerId).FirstOrDefaultAsync()
                    : null;

                data.Add(new ExtenTable
                {
                    Id = table.Id,
                    Capacity = table.Capacity,
                    CreatedAt = table.CreatedAt,
                    IsDeleted = table.IsDeleted,
                    IsReservable = table.IsReservable,
                    Location = table.Location,
                    Note = table.Note,
                    ParentId = table.ParentId,
                    ReservedFrom = table.ReservedFrom,
                    ReservedTo = table.ReservedTo,
                    Status = table.Status,
                    TableName = table.TableName,
                    Tags = table.Tags,
                    Type = table.Type,
                    UpdatedAt = table.UpdatedAt,
                    CreatedAto = order?.CreatedAt,
                    SumQuantity = totalQuantity,
                    CustomerName = customer?.FullName,
                    Total = total,
                });
            }

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] TableDTO tableDTO)
        {
            var entity = ToEntity(tableDTO);
            await _table.InsertOneAsync(entity);
            await _hubContext.Clients.All.SendAsync("loadTable", entity);
            return Ok(entity);
        }
        private Table ToEntity(TableDTO dto)
        {
            return new Table
            {
                TableName = dto.TableName,
                Capacity = dto.Capacity,
                Status = dto.Status,
                Type = dto.Type,
                Location = dto.Location,
                Note = dto.Note,
                IsDeleted = dto.IsDeleted ?? false,
                IsReservable = dto.IsReservable,
                Tags = dto.Tags,
                ParentId = dto.ParentId,
            };
        }
        public class ExtenTable : TableDTO
        {
            public int SumQuantity { get; set; }
            public string CustomerName { get; set; }
            public DateTime? CreatedAto { get; set; }
            public decimal Total { get; set; }

        }

    }
}
