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
                OrderId = order.Id,
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
            public string? OrderId { get; set; }
            public string? Code { get; set; }
            public string? TableId { get; set; }
            public string? TableName { get; set; }
            public decimal? TotalAmount { get; set; }
            public string? FullNameEmployee { get; set; }
            public string? FullNameCustomer { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class CreateOrderRequest
        {
            public OrderDTO? OrderDto { get; set; }
            public CustomerDTO? CustomerDto { get; set; }
        }


        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // add customer
            if(request.CustomerDto != null)
            {
                var entityCustomer = ToEntityCustomer(request.CustomerDto);
                await _customer.InsertOneAsync(entityCustomer);
                request.OrderDto.CustomerId = entityCustomer.Id;
            }
            // add order 
            var entityOrder = ToEntityOrder(request.OrderDto);
            await _order.InsertOneAsync(entityOrder);

            // update bàn
            var table = _table.Find(d => d.Id == request.OrderDto.TableId).FirstOrDefault();
            table.Status = "occupied";
            await _table.ReplaceOneAsync(d => d.Id == table.Id, table);
            await _hubContext.Clients.All.SendAsync("loadTable", table);
            return Ok(entityOrder);
        }

        private Order ToEntityOrder(OrderDTO dto)
        {
            if (dto == null) return null;
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

        private Customer ToEntityCustomer(CustomerDTO dto)
        {
            if (dto == null) return null;

            return new Customer
            {
                FullName = dto.FullName ?? null,
                PhoneNumber = dto.PhoneNumber ?? null,
                Email = dto.Email ?? null,
                Password = dto.Password ?? null,
                DateOfBirth = dto.DateOfBirth ?? null,
                Gender = dto.Gender ?? null,
                Address = dto.Address ?? null,
                MembershipLevel = dto.MembershipLevel ?? "Thường",
                RewardPoints = dto.RewardPoints ?? 0,
                CreatedAt = dto.CreatedAt ?? DateTime.Now,
                Role = dto.Role ?? null,
                IsDeleted = dto.IsDeleted ?? false,
                Note = dto.Note ?? null
            };
        }

    }
}
