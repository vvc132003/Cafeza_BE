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
        private readonly IMongoCollection<OrderDetail> _orderdetail;
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<OrderCancellation> _orderCancellation;
        private readonly IMongoCollection<TableTransfer> _tableTransfer;


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
            _orderdetail = context.OrderDetails;
            _drink = context.Drinks;
            _orderCancellation = context.OrderCancellations;
            _tableTransfer = context.TableTransfers;
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


        [HttpPost("createOrder")]
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

        public class UpdateCancelOrderRequest 
        {
            //public string? OrderId { get; set; }
            public OrderCancellationDTO? OrderCancellationDTO { get; set; }
        }

        //[HttpGet("updateCancelOrder/{orderId}")]
        [HttpPost("updateCancelOrder")]
        public async Task<IActionResult> UpdateCancelOrder([FromBody] OrderCancellationDTO orderCancellationDTO)
        {
            var order = await _order.Find(o => o.Id == orderCancellationDTO.OrderId).FirstOrDefaultAsync();
            order.Status = "Đã huỷ";
            await _order.ReplaceOneAsync(d => d.Id == order.Id, order);
            var table = _table.Find(d => d.Id == order.TableId).FirstOrDefault();
            table.Status = "empty";
            await _table.ReplaceOneAsync(d => d.Id == table.Id, table);

            var orderdetails = await _orderdetail.Find(or => or.OrderId == orderCancellationDTO.OrderId).ToListAsync();
            if(orderdetails != null)
            {
                var drinkIds = orderdetails.Select(od => od.DrinkId).Distinct().ToList();
                var drinks = await _drink.Find(d => drinkIds.Contains(d.Id)).ToListAsync();

                var drinkDict = drinks.ToDictionary(d => d.Id);

                foreach (var orderdetail in orderdetails)
                {
                    if (drinkDict.TryGetValue(orderdetail.DrinkId, out var drink))
                    {
                        drink.Quantity += orderdetail.Quantity;
                        await _drink.ReplaceOneAsync(d => d.Id == drink.Id, drink);
                    }
                }
            }

            var orderCancellation = ToEntityOrderCancellation(orderCancellationDTO);
            await _orderCancellation.InsertOneAsync(orderCancellation);

            await _hubContext.Clients.All.SendAsync("loadTable", table);
            return Ok(order);
        }

        private OrderCancellation ToEntityOrderCancellation(OrderCancellationDTO dto)
        {
            if (dto == null) return null;
            return new OrderCancellation
            {
                OrderId = dto.OrderId,
                Reason = dto.Reason,
                CancelTime = dto.CancelTime,
                EmployeeId = dto.EmployeeId
            };
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
                Note = dto.Note,
                AmountPaid = dto.AmountPaid,
                ChangeAmount = dto.ChangeAmount,
                PaidAt = null
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

        public class ChangeTableRequest
        {
            public string FromTableId { get; set; }
            public string ToTableId { get; set; }
            public string EmployeeId { get; set; }
        }

        [HttpPost("changeTable")]
        public async Task<IActionResult> ChangeTable([FromBody] ChangeTableRequest data)
        {
            // cập nhật table cũ
            //var tablefrom = await _table.Find(t => t.Id == data.FromTableId).FirstOrDefaultAsync();
            //tablefrom.Status = "empty";
            //await _table.ReplaceOneAsync(d => d.Id == tablefrom.Id, tablefrom);
            await _table.UpdateOneAsync(
                                        t => t.Id == data.FromTableId,
                                        Builders<Table>.Update.Set(t => t.Status, "empty")
                                    );


            // cập nhật table mới
            var tableto = await _table.Find(t => t.Id == data.ToTableId).FirstOrDefaultAsync();
            //tableto.Status = "occupied";
            //await _table.ReplaceOneAsync(d => d.Id == tableto.Id, tableto);
            await _table.UpdateOneAsync(
                                        t => t.Id == data.ToTableId,
                                        Builders<Table>.Update.Set(t => t.Status, "occupied")
                                    );

            // cập nhật order
            var order = await _order.Find(o => o.TableId == data.FromTableId).FirstOrDefaultAsync();
            //order.TableId = data.ToTableId;
            //await _order.ReplaceOneAsync(o => o.Id == order.Id, order);
            await _order.UpdateOneAsync(
                                        o => o.Id == order.Id,
                                        Builders<Order>.Update.Set(o => o.TableId, data.ToTableId)
                                    );

            // lưu thông tin chuyển bàn

            var tableTransferdto = new TableTransferDTO();
            if(data.ToTableId != null)
            {
                tableTransferdto.OrderId = order.Id;
                tableTransferdto.FromTableId = data.FromTableId;
                tableTransferdto.ToTableId = data.ToTableId;
                tableTransferdto.EmployeeId = data.EmployeeId;
                var tableTransfer = toEntityTableTransfer(tableTransferdto);
                await _tableTransfer.InsertOneAsync(tableTransfer);
            }

            //await _hubContext.Clients.All.SendAsync("loadTable", tablefrom);
            await _hubContext.Clients.All.SendAsync("loadTable", tableto);
            return Ok();
        }
        private TableTransfer toEntityTableTransfer(TableTransferDTO dto)
        {
            return new TableTransfer
            {
                OrderId = dto.OrderId,
                FromTableId = dto.FromTableId,
                ToTableId = dto.ToTableId,
                TransferTime = dto.TransferTime,
                EmployeeId = dto.EmployeeId,
                Note = dto.Note
            };
        }

        public class PayRequest
        {
            public decimal? TotalAmount { get; set; }
            public decimal? AmountPaid { get; set; }      // Khách trả bao nhiêu
            public decimal? ChangeAmount { get; set; }    // Tiền thối lại
            public string? PaymentMethod { get; set; }
            public string? OrderId { get; set; }

        }

        [HttpPost("pay")]
        public async Task<IActionResult> PayOrder([FromBody] PayRequest data)
        {
            var order = await _order.Find(o => o.Id == data.OrderId).FirstOrDefaultAsync();
            if (order == null)
                return NotFound("Order not found");

            var update = Builders<Order>.Update
                .Set(o => o.TotalAmount, data.TotalAmount)
                .Set(o => o.AmountPaid, data.AmountPaid)
                .Set(o => o.ChangeAmount, data.ChangeAmount)
                .Set(o => o.PaymentMethod, data.PaymentMethod)
                .Set(o => o.Status, "Đã thanh toán")
                .Set(o => o.PaidAt, DateTime.Now);

            await _order.UpdateOneAsync(o => o.Id == data.OrderId, update);

            await _table.UpdateOneAsync(
                                       t => t.Id == order.TableId,
                                       Builders<Table>.Update.Set(t => t.Status, "empty")
                                   );
            var table = await _table.Find(t => t.Id == order.TableId).FirstOrDefaultAsync();

            await _hubContext.Clients.All.SendAsync("loadTable", table);


            return Ok();
        }


    }
}
