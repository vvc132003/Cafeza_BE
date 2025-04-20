using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using MongoDB.Driver;
using static Cafeza_BE.Controllers.OrderController;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Table> _table;
        private readonly IMongoCollection<Order> _order;
        private readonly IMongoCollection<Customer> _customer;
        private readonly IMongoCollection<Employee> _employee;
        private readonly IHubContext<SignalRHub> _hubContext;

        public OrderController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _table = context.Tables;
            _hubContext = hubContext;
            _order = context.Orders;
            _customer = context.Customers;
            _employee = context.Employees;
        }


        [HttpGet("table/{tableId}")]
        public async Task<IActionResult> GetOrdersByTableId(string tableId)
        {
            var data = new List<object>();
            var order = await _order.Find(x => x.TableId == tableId && x.Status == "Chờ thanh toán")
                                       .SortByDescending(x => x.CreatedAt)
                                       .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng cho bàn này.");
            }
            var table = await _table.Find(d => d.Id == order.TableId).FirstOrDefaultAsync();
            var customer = await _customer.Find(x => x.Id == order.CustomerId).FirstOrDefaultAsync();
            var employee = await _employee.Find(x => x.Id == order.EmployeeId).FirstOrDefaultAsync();
            data.Add(new ExtenOrder
            {
                Id = order.Id,
                Code = order.Code,
                TableId = tableId,
                TableName = table.TableName,
                TotalAmount = order.TotalAmount,
                FullNameCustomer = customer?.FullName ?? "Khách lẻ",
                FullNameEmployee = employee?.FullName ?? "Nhân viên không xác định",
                CreatedAt = order.CreatedAt,
            });
            return Ok(data);
        }

        public class ExtenOrder
        {
            public string? Id { get; set; }
            public string? Code { get; set; }
            public string? TableId { get; set; }
            public string? TableName { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? FullNameEmployee { get; set; }
            public string? FullNameCustomer { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO orderDTO)
        {
            var entity = ToEntity(orderDTO);
            await _order.InsertOneAsync(entity);
            var table = _table.Find(d => d.Id == orderDTO.TableId).FirstOrDefault();
            table.Status = "occupied";
            await _table.ReplaceOneAsync(d => d.Id == table.Id, table);
            await _hubContext.Clients.All.SendAsync("loadTable", table);
            return Ok(entity);
        }

        private Order ToEntity(OrderDTO dto)
        {
            return new Order
            {
                CreatedAt = DateTime.Now,
                Code = dto.Code,
                CustomerId = dto.CustomerId,
                EmployeeId = dto.EmployeeId,
                TableId = dto.TableId,
                TotalAmount = dto.TotalAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = "Chờ thanh toán",
                Note = dto.Note
            };
        }

    }
}
