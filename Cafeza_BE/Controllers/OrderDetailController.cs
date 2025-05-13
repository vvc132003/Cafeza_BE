using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IMongoCollection<Table> _table;
        private readonly IMongoCollection<Order> _order;
        private readonly IMongoCollection<OrderDetail> _orderDetail;
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<Customer> _customer;
        private readonly IMongoCollection<Employee> _employee;
        private readonly IHubContext<SignalRHub> _hubContext;

        public OrderDetailController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _table = context.Tables;
            _hubContext = hubContext;
            _order = context.Orders;
            _customer = context.Customers;
            _employee = context.Employees;
            _orderDetail = context.OrderDetails;
            _drink = context.Drinks;
        }

        [HttpGet("orderId/{orderId}")]
        public async Task<IActionResult> GetOrdersByOrderId(string orderId)
        {
            var data = new List<object>();
            var orderDetails = await _orderDetail.Find(o => o.OrderId == orderId).ToListAsync();
            //Lấy danh sách các DrinkId duy nhất
            var drinkIds = orderDetails.Select(o => o.DrinkId).Distinct().ToList();
            //Lấy thông tin đồ uống một lần
            var drinks = await _drink.Find(d => drinkIds.Contains(d.Id)).ToListAsync();
            //Chuyển sang Dictionary để tra nhanh
            var drinkDict = drinks.ToDictionary(d => d.Id, d => d);

            foreach (var item in orderDetails) {
                var drinkName = drinkDict.ContainsKey(item.DrinkId) ? drinkDict[item.DrinkId].Name : "Không xác định";
                data.Add(new ExtenOrderDetail
                {
                    OrderdetailId = item.Id,
                    OrderId = item.OrderId,
                    DrinkId = item.DrinkId,
                    DrinkName = drinkName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.Total,
                    Note = item.Note,
                });
            }
            return Ok(data);
        }

        public class CreateOrderDetailRequest
        {
            public OrderDetailDTO? OrderDetailDto { get; set; }
            public DrinkDTO? drinkDTO { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderDetail([FromBody] CreateOrderDetailRequest request)
        {
            if (request == null) {
                return null;
            }
            var orderdetail = await _orderDetail.Find(or => or.OrderId == request.OrderDetailDto.OrderId && or.DrinkId == request.OrderDetailDto.DrinkId).FirstOrDefaultAsync();
            if(orderdetail == null) {
                var entityOrderdetail = ToEntity(request.OrderDetailDto);
                await _orderDetail.InsertOneAsync(entityOrderdetail);
                var result = ToExtenOrderDetail(entityOrderdetail, request.drinkDTO);
                return Ok(result);

            }
            else
            {
                orderdetail.Quantity += 1;
                orderdetail.Total = orderdetail.UnitPrice * orderdetail.Quantity;
                await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);
                var result = ToExtenOrderDetail(orderdetail, request.drinkDTO);
                return Ok(result);


            }

        }
        private ExtenOrderDetail ToExtenOrderDetail(OrderDetail entity, DrinkDTO drink)
        {
            return new ExtenOrderDetail
            {
                OrderdetailId = entity.Id,
                OrderId = entity.OrderId,
                DrinkId = entity.DrinkId,
                DrinkName = drink.Name,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                Total = entity.Total,
                Note = entity.Note,
            };
        }

        private OrderDetail ToEntity(OrderDetailDTO dto)
        {
            if (dto == null) return null;
            return new OrderDetail
            {
                OrderId = dto.OrderId,
                DrinkId = dto.DrinkId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Total = dto.UnitPrice * dto.Quantity,
                Note = dto.Note
            };
        }


        public class ExtenOrderDetail
        {
            public string? OrderdetailId { get; set; }
            public string? OrderId { get; set; }
            public string? DrinkId { get; set; }
            public string? DrinkName { get; set; }
            public int? Quantity { get; set; }
            public decimal? UnitPrice { get; set; }
            public decimal? Total;
            public string? Note { get; set; }
        }
    }
}
