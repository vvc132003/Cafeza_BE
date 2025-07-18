using System.Threading.Tasks;
using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using MongoDB.Driver;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using static QuestPDF.Helpers.Colors;

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
        private readonly IMongoCollection<CustomerDetails> _customer;
        private readonly IMongoCollection<EmployeeDetails> _employee;
        private readonly IMongoCollection<User> _user;

        private readonly IHubContext<SignalRHub> _hubContext;

        public OrderDetailController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _table = context.Tables;
            _hubContext = hubContext;
            _order = context.Orders;
            _customer = context.CustomerDetails;
            _employee = context.EmployeeDetails;
            _orderDetail = context.OrderDetails;
            _user = context.Users;
            _drink = context.Drinks;
        }

        [HttpGet("orderId/{orderId}")]
        public async Task<IActionResult> GetOrdersByOrderId(string orderId)
        {
            var data = new List<object>();
            var orderDetails = await _orderDetail.Find(o => o.OrderId == orderId && o.Status != "delete").ToListAsync();
            //Lấy danh sách các DrinkId duy nhất
            var drinkIds = orderDetails.Select(o => o.DrinkId).Distinct().ToList();
            //Lấy thông tin đồ uống một lần
            var drinks = await _drink.Find(d => drinkIds.Contains(d.Id)).ToListAsync();
            //Chuyển sang Dictionary để tra nhanh
            var drinkDict = drinks.ToDictionary(d => d.Id, d => d);

            foreach (var item in orderDetails) {
                var drinkName = drinkDict.ContainsKey(item.DrinkId) ? drinkDict[item.DrinkId].Name : "Không xác định";
                data.Add(new ExtenOrderDetailStatus
                {
                    OrderdetailId = item.Id,
                    OrderId = item.OrderId,
                    DrinkId = item.DrinkId,
                    DrinkName = drinkName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.Total,
                    Note = item.Note,
                    Status = item.Status
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
            var orderdetail = await _orderDetail.Find(or => or.OrderId == request.OrderDetailDto.OrderId && or.DrinkId == request.OrderDetailDto.DrinkId &&
            (or.Status == null || or.Status == "waiting")).FirstOrDefaultAsync();
            var drink = await _drink.Find(dr => dr.Id == request.OrderDetailDto.DrinkId).FirstOrDefaultAsync();
            drink.Quantity -= 1;
            await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);
            if (orderdetail == null) {
                var entityOrderdetail = ToEntity(request.OrderDetailDto);
                entityOrderdetail.CreatedAt = DateTime.UtcNow;
                await _orderDetail.InsertOneAsync(entityOrderdetail);
                var result = await ToExtenOrderDetail(entityOrderdetail, request.drinkDTO);
                await _hubContext.Clients.Group(entityOrderdetail.OrderId).SendAsync("LoadOrderId", result);
                await _hubContext.Clients.All.SendAsync("loadStatusOrderdetail", result);
                return Ok(result);

            }
            else
            {
                orderdetail.Quantity += 1;
                orderdetail.Total = orderdetail.UnitPrice * orderdetail.Quantity;
                await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);
                var result = await ToExtenOrderDetail(orderdetail, request.drinkDTO);
                await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("LoadOrderId", result);
                await _hubContext.Clients.All.SendAsync("loadStatusOrderdetail", result);
                return Ok(result);
            }

        }

        public class UpdateQuantityOrderDetailRequest
        {
            public int Quantity { get; set; }
            public string Note { get; set; }
            public ExtenOrderDetail? ExtenOrderDetail { get; set; }
        }

        //[HttpPut]
        //public async Task<IActionResult> UpdateQuantityOrderDetail([FromBody] UpdateQuantityOrderDetailRequest request)
        //{
        //    if (request == null)
        //    {
        //        return null;
        //    }


        //    var orderdetail = await _orderDetail.Find(o => o.OrderId == request.ExtenOrderDetail.OrderId && o.DrinkId == request.ExtenOrderDetail.DrinkId).FirstOrDefaultAsync();
        //    var drink = await _drink.Find(dr => dr.Id == orderdetail.DrinkId).FirstOrDefaultAsync();

        //    if (request.Quantity > request.ExtenOrderDetail?.Quantity)
        //    {
        //        orderdetail.Quantity += request.Quantity;
        //        orderdetail.Note = request.Note;
        //        orderdetail.Total = orderdetail.UnitPrice * orderdetail.Quantity;
        //        await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);
        //        drink.Quantity -= request.Quantity;
        //        await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);
        //        var result = ToExtenOrderDetail(orderdetail, drink);
        //        await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("LoadOrderId", result);
        //    }
        //    else
        //    {
        //        orderdetail.Quantity -= request.Quantity;
        //        orderdetail.Note = request.Note;
        //        orderdetail.Total = orderdetail.UnitPrice * orderdetail.Quantity;
        //        await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);
        //        drink.Quantity -= request.Quantity;
        //        await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);
        //        var result = ToExtenOrderDetail(orderdetail, drink);
        //        await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("LoadOrderId", result);
        //    }

        //    return Ok();
        //}
        [HttpPut]
        public async Task<IActionResult> UpdateQuantityOrderDetail([FromBody] UpdateQuantityOrderDetailRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request is null.");
            }

            var orderdetail = await _orderDetail
                .Find(o => o.OrderId == request.ExtenOrderDetail.OrderId && o.DrinkId == request.ExtenOrderDetail.DrinkId)
                .FirstOrDefaultAsync();

            var drink = await _drink
                .Find(dr => dr.Id == orderdetail.DrinkId)
                .FirstOrDefaultAsync();

            if (orderdetail == null || drink == null)
            {
                return NotFound("Order detail or drink not found.");
            }

            int oldQuantity = orderdetail.Quantity;
            int newQuantity = request.Quantity;
            int delta = newQuantity - oldQuantity;

            orderdetail.Quantity = newQuantity;
            orderdetail.Note = request.Note;
            orderdetail.Total = orderdetail.UnitPrice * newQuantity;

            await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);

            drink.Quantity -= delta;
            await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);

            var result = await ToExtenOrderDetail(orderdetail, drink);
            await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("LoadOrderId", result);

            return Ok();
        }

        [HttpGet("updateStatusdetail/{orderdetailId}/{status}")]
        public async Task<IActionResult> UpdateStatusdetail(string orderdetailId, string status)
        {
            var orderdetail = await _orderDetail.Find(o => o.Id == orderdetailId).FirstOrDefaultAsync();
            orderdetail.Status = status;
            await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetailId, orderdetail);
            return Ok();
        }

        [HttpGet("exportInvoice/{orderId}")]
        public async Task<IActionResult> ExportInvoice(string orderId)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var order = await _order.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order == null)
                return NotFound();

            var orderDetails = await _orderDetail.Find(d => d.OrderId == orderId).ToListAsync();

            var drinkIds = orderDetails.Select(x => x.DrinkId).Distinct().ToList();
            var drinks = await _drink.Find(d => drinkIds.Contains(d.Id)).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A5);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Element(e =>
                    {
                        e.PaddingBottom(10).Text("HÓA ĐƠN: " + order.Code)
                         .FontSize(16).Bold().AlignCenter();
                    });


                    page.Content().Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text($"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        //col.Item().Element(e =>
                        //{
                        //    e.PaddingVertical(5).LineHorizontal(1);
                        //});
                        col.Item().PaddingBottom(10).LineHorizontal(1);


                        col.Item().Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingHorizontal(10)
                        .Row(row =>
                        {
                            row.RelativeColumn().Text("Tên đồ uống").Bold().FontSize(10);
                            row.ConstantColumn(60).AlignCenter().Text("Giá bán").Bold().FontSize(10);
                            row.ConstantColumn(60).AlignCenter().Text("SL").Bold().FontSize(10);
                            row.ConstantColumn(80).AlignRight().Text("Thành tiền").Bold().FontSize(10);
                        });


                        foreach (var item in orderDetails)
                        {
                            var drink = drinks.FirstOrDefault(d => d.Id == item.DrinkId);
                            if (drink == null) continue;

                            col.Item().PaddingVertical(5).PaddingHorizontal(10)
                                .Row(row =>
                                {
                                    row.RelativeColumn().Text(drink.Name).FontSize(10);
                                    row.ConstantColumn(60).AlignCenter().Text($"{drink.Price:N0} VNĐ").FontSize(10);
                                    row.ConstantColumn(60).AlignCenter().Text($"{item.Quantity}").FontSize(10);
                                    row.ConstantColumn(80).AlignRight().Text($"{item.UnitPrice * item.Quantity:N0} VNĐ").FontSize(10);
                                });
                        }

                        //col.Item().Element(e =>
                        //{
                        //    e.PaddingTop(5).LineHorizontal(1);
                        //});
                        col.Item().PaddingTop(10).LineHorizontal(1);

                        var totalAmount = order.TotalAmount;
                        if (totalAmount == 0)
                        {
                            totalAmount = orderDetails.Sum(item => item.UnitPrice * item.Quantity);
                        }
                        col.Item()
                        .PaddingTop(10) 
                        .AlignRight()
                        .Text($"Tổng cộng: {totalAmount:N0} VNĐ")
                        .Bold()
                        .FontSize(14);

                    });

                    page.Footer().AlignCenter().Text("Cảm ơn quý khách!").Italic();
                });
            });

            var pdfBytes = document.GeneratePdf();

            // Tạo đường dẫn lưu file
            string directoryPath = @"D:\Invoices";
            Directory.CreateDirectory(directoryPath); // tạo thư mục nếu chưa có

            string filePath = Path.Combine(directoryPath, $"hoadon_{order.Code}.pdf");

            // Ghi file ra đĩa D
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // Trả kết quả thành công (nếu không cần trả file về frontend)
            return Ok(new { message = "Hóa đơn đã được lưu tại " + filePath });
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var orderdetail = await _orderDetail.Find(or => or.Id == id).FirstOrDefaultAsync();
            var drink = await _drink.Find(dr => dr.Id == orderdetail.DrinkId).FirstOrDefaultAsync();
            if (orderdetail.Quantity > 1)
            {
                orderdetail.Quantity -= 1;
                orderdetail.Total = orderdetail.UnitPrice * orderdetail.Quantity;
                await _orderDetail.ReplaceOneAsync(x => x.Id == orderdetail.Id, orderdetail);
                drink.Quantity += 1;
                await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);
                var result = await ToExtenOrderDetail(orderdetail, drink);
                await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("LoadOrderId", result);
            }
            else
            {
                await _orderDetail.DeleteOneAsync(x => x.Id == orderdetail.Id);
                drink.Quantity += 1;
                await _drink.ReplaceOneAsync(x => x.Id == drink.Id, drink);
                var result = await ToExtenOrderDetail(orderdetail, drink);    
                await _hubContext.Clients.Group(orderdetail.OrderId).SendAsync("RemoveOrderDetailId", result);
            }
            

            return NoContent();
        }


        [HttpGet("getAllOrdersDetail")]
        public async Task<IActionResult> GetAllOrdersDetail()
        {
            var data = new List<object>();

            //var orderDetails = await _orderDetail.Find(Builders<OrderDetail>.Filter.Empty).ToListAsync();
            var orderDetails = await _orderDetail
                .Find(o => o.Status != null && o.Status != "")
                .ToListAsync();

            foreach (var orderdetail in orderDetails) {
                var drink = await _drink.Find(d => d.Id == orderdetail.DrinkId).FirstOrDefaultAsync();
                var order = await _order.Find(d => d.Id == orderdetail.OrderId).FirstOrDefaultAsync();
                var table = await _table.Find(d => d.Id == order.TableId).FirstOrDefaultAsync();

                data.Add(new ExtenOrderDetailStatus
                {
                    OrderId = orderdetail.OrderId,
                    OrderdetailId = orderdetail.Id,
                    DrinkId = orderdetail.DrinkId,
                    DrinkName = drink.Name,
                    Quantity = orderdetail.Quantity,
                    UnitPrice = orderdetail.UnitPrice,
                    Total  = orderdetail.Total,
                    Status = orderdetail.Status,
                    Note = orderdetail.Note,
                    TableName = table.TableName,
                    CreatedAt = orderdetail.CreatedAt,
                });
            }
            return Ok(data);
        }

        private async Task<ExtenOrderDetailStatus> ToExtenOrderDetail(OrderDetail entity, object drink)
        {
            string drinkName = null;

            if (drink is DrinkDTO drinkDto)
                drinkName = drinkDto.Name;
            else if (drink is Drink drinkEntity)
                drinkName = drinkEntity.Name;

            var order = await _order.Find(o => o.Id == entity.OrderId).FirstOrDefaultAsync();
            var table = await _table.Find(t => t.Id == order.TableId).FirstOrDefaultAsync();

            return new ExtenOrderDetailStatus
            {
                OrderdetailId = entity.Id,
                OrderId = entity.OrderId,
                DrinkId = entity.DrinkId,
                DrinkName = drinkName,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                Total = entity.Total,
                Note = entity.Note,
                Status = entity.Status,
                TableName = table.TableName
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
                Note = dto.Note,
                Status = dto.Status,
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
            public decimal? Total { get; set; }
            public string? Note { get; set; }
        }
        public class ExtenOrderDetailStatus : ExtenOrderDetail
        {
            public string? Status { get; set; } // kế thừa vì có nhiều dữ liệu trùng lặp
            public string? TableName { get; set; }
            public DateTime? CreatedAt { get; set; }

        }
    }
}
