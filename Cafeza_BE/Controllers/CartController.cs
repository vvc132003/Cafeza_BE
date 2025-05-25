using System.Threading.Tasks;
using Cafeza_BE.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using MongoDB.Driver;
using static Cafeza_BE.Controllers.CartController;
using static Cafeza_BE.Controllers.OrderDetailController;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<Category> _category;
        private readonly IMongoCollection<Cart> _cart;
        private readonly IMongoCollection<CartDetail> _cartdetail;
        private readonly IMongoCollection<User> _user;


        public CartController(MongoDbContext context)
        {
            _drink = context.Drinks;
            _category = context.Categorys;
            _cart = context.Carts;
            _cartdetail = context.CartDetails;
            _user = context.Users;
        }

        [HttpGet("getCurrentCartByUserIdAsync/{userId}")]
        public async Task<IActionResult> GetCurrentCartByUserIdAsync(string userId)
        {
            var cart = await _cart.Find(ca => ca.CustomerId == userId && ca.Status == "10").FirstOrDefaultAsync();
            if(cart != null)
            {
                var cartLis = await GetCurrentCartByID(cart.Id);
                return Ok(cartLis);
            }

            var cartId = await CreateCartAsync(userId);

            //var drink = await _drink.Find(d => d.Id == drinkId).FirstOrDefaultAsync();
            //var cartdetailDTO = new CartDetailDTO();
            //cartdetailDTO.CartId = newcart.Id;
            //cartdetailDTO.DrinkId = drinkId;
            //cartdetailDTO.Status = "90";
            //cartdetailDTO.UnitPrice = drink.Price;
            //cartdetailDTO.Quantity = 1;
            //cartdetailDTO.Total = cartdetailDTO.Quantity * cartdetailDTO.UnitPrice;
            //var newCartdetail = ToEntityCartDetail(cartdetailDTO);
            //await _cartdetail.InsertOneAsync(newCartdetail);

            return Ok(cartId);
        }


        private async Task<List<object>> GetCurrentCartByID(string id)
        {
            var data = new List<object>();
            var cartdetails = await _cartdetail.Find(cd => cd.CartId == id).ToListAsync();
            foreach(var cartdetail in cartdetails)
            {
                var dink = await _drink.Find(cd => cd.Id == cartdetail.DrinkId).FirstOrDefaultAsync();
                data.Add(new ExtenCart
                {
                    Id = cartdetail.Id,
                    CartId = cartdetail.CartId,
                    DrinkId = dink.Id,
                    DrinkName = dink.Name,
                    Quantity = cartdetail.Quantity,
                    UnitPrice = cartdetail.UnitPrice,
                    Total = cartdetail.Quantity * cartdetail.UnitPrice,
                    Note = cartdetail.Note,
                    Status = cartdetail.Status,
                    Image = dink.ImagePath,
                });
            }
            return data;
        }

        private Cart ToEntityCart(CartDTO dto)
        {
            if (dto == null) return null;
            return new Cart
            {
                CreatedAt = DateTime.Now,
                Code = dto.Code,
                CustomerId = dto.CustomerId,
                TotalAmount = dto.TotalAmount,
                Status = "10",
                Note = dto.Note,
            };
        }


        private async Task<string> CreateCartAsync(string userId)
        {
            var cartDTO = new CartDTO
            {
                CustomerId = userId,
                Status = "10",
                TotalAmount = 0
            };

            var newcart = ToEntityCart(cartDTO);

            await _cart.InsertOneAsync(newcart);

            return newcart.Id;
        }

        public class CdetailRes
        {
            public Drink Drink { get; set; }
            public string UserId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCartandDetail(CdetailRes res)
        {
            var cart = await _cart.Find(ca => ca.CustomerId == res.UserId && ca.Status == "10").FirstOrDefaultAsync();

            string cartId;

            if (cart == null)
            {
                cartId = await CreateCartAsync(res.UserId);
            }
            else
            {
                cartId = cart.Id;
            }

            var drink = await _drink.Find(d => d.Id == res.Drink.Id).FirstOrDefaultAsync();
            var update = Builders<Drink>.Update.Inc(d => d.Quantity, -1);
            await _drink.UpdateOneAsync(d => d.Id == res.Drink.Id, update);

            if (drink.Quantity ==0) {
                return Ok(false);
            }

            var existingCartDetail = await _cartdetail
                         .Find(cd => cd.CartId == cartId && cd.DrinkId == drink.Id && cd.Status == "90")
                         .FirstOrDefaultAsync();

            if (existingCartDetail != null)
            {
                existingCartDetail.Quantity += 1;
                existingCartDetail.Total = existingCartDetail.Quantity * existingCartDetail.UnitPrice;

                var filter = Builders<CartDetail>.Filter.Eq(cd => cd.Id, existingCartDetail.Id);
                await _cartdetail.ReplaceOneAsync(filter, existingCartDetail);
                var result = ToExtenCartDetail(existingCartDetail,drink);
                return Ok(result);
            }
            else
            {
                var cartdetailDTO = new CartDetailDTO
                {
                    CartId = cartId,
                    DrinkId = drink.Id,
                    Status = "90",
                    UnitPrice = drink.Price,
                    Quantity = 1,
                    Total = drink.Price
                };

                var newCartDetail = ToEntityCartDetail(cartdetailDTO);
                await _cartdetail.InsertOneAsync(newCartDetail);
                var result = ToExtenCartDetail(newCartDetail, drink);
                return Ok(result);

            }
        }

        private ExtenCart ToExtenCartDetail(CartDetail entity, object drink)
        {
            string drinkName = "";
            string image = "";

            if (drink is DrinkDTO drinkDto)
            {
                drinkName = drinkDto.Name;  
                image = drinkDto.ImagePath;    
            }
            else if (drink is Drink drinkEntity)
            {
                drinkName = drinkEntity.Name;  
                image = drinkEntity.ImagePath;     
            }

            return new ExtenCart
            {
                Id = entity.Id,
                CartId = entity.CartId,
                DrinkId = entity.DrinkId,
                DrinkName = drinkName,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                Total = entity.Total,
                Note = entity.Note,
                Status = entity.Status,
                Image = image
            };
        }


        public class ExtenCart
        {
            public string Id { get; set; }
            public string? CartId { get; set; }
            public string DrinkId { get; set; }
            public string DrinkName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total;
            public string? Note { get; set; }
            public string? Status { get; set; }
            public string ? Image { get; set; }
        }


        private CartDetail ToEntityCartDetail(CartDetailDTO dto)
        {
            if (dto == null) return null;
            return new CartDetail
            {
                CartId = dto.CartId,
                DrinkId = dto.DrinkId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Total = dto.Total,
                Status = "90",
                Note = dto.Note,
            };
        }

        [HttpGet("decreaseQuantity/{id}")]
        public async Task<IActionResult> DecreaseQuantity(string id)
        {
            var cartdetail = await _cartdetail.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (cartdetail == null) return NotFound();
            var drink = await _drink.Find(d => d.Id == cartdetail.DrinkId).FirstOrDefaultAsync();
            if (drink == null) return NotFound();
            var update = Builders<Drink>.Update.Inc(d => d.Quantity, +1);
            await _drink.UpdateOneAsync(d => d.Id == cartdetail.DrinkId, update);
            cartdetail.Quantity -= 1;
            cartdetail.Total = cartdetail.Quantity * drink.Price;
            var updateCartDetail = Builders<CartDetail>.Update
                                     .Set(c => c.Quantity, cartdetail.Quantity)
                                     .Set(c => c.Total, cartdetail.Total);

            await _cartdetail.UpdateOneAsync(c => c.Id == cartdetail.Id, updateCartDetail);

            return Ok();
        }

        [HttpGet("increaseQuantity/{id}")]
        public async Task<IActionResult> IncreaseQuantity(string id)
        {
            var cartdetail = await _cartdetail.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (cartdetail == null) return NotFound();
            var drink = await _drink.Find(d => d.Id == cartdetail.DrinkId).FirstOrDefaultAsync();
            if (drink == null) return NotFound();
            var update = Builders<Drink>.Update.Inc(d => d.Quantity, -1);
            await _drink.UpdateOneAsync(d => d.Id == cartdetail.DrinkId, update);
            cartdetail.Quantity += 1;
            cartdetail.Total = cartdetail.Quantity * drink.Price;
            var updateCartDetail = Builders<CartDetail>.Update
                                     .Set(c => c.Quantity, cartdetail.Quantity)
                                     .Set(c => c.Total, cartdetail.Total);

            await _cartdetail.UpdateOneAsync(c => c.Id == cartdetail.Id, updateCartDetail);

            return Ok();
        }

        /// hàm decreaseQuantity và increaseQuantity gộp thành hàm changeQuantity

        [HttpGet("changeQuantity/{drinkId}/{cartId}")]
        public async Task<IActionResult> ChangeQuantity(string drinkId, string cartId, [FromQuery] int change)
        {
            var cartdetail = await _cartdetail.Find(c => c.DrinkId == drinkId && c.CartId == cartId).FirstOrDefaultAsync();
            if (cartdetail == null) return NotFound("Cart detail not found.");

            var drink = await _drink.Find(d => d.Id == drinkId).FirstOrDefaultAsync();
            if (drink == null) return NotFound("Drink not found.");

            int newQuantity = cartdetail.Quantity + change;
            if (newQuantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }

            int stockChange = -change;

            if (change > 0 && drink.Quantity < change)
            {
                return BadRequest("Not enough stock available.");
            }

            var updateDrink = Builders<Drink>.Update.Inc(d => d.Quantity, stockChange);
            await _drink.UpdateOneAsync(d => d.Id == drink.Id, updateDrink);

            cartdetail.Quantity = newQuantity;
            cartdetail.Total = cartdetail.Quantity * drink.Price;

            var updateCartDetail = Builders<CartDetail>.Update
                .Set(c => c.Quantity, cartdetail.Quantity)
                .Set(c => c.Total, cartdetail.Total);

            await _cartdetail.UpdateOneAsync(c => c.Id == cartdetail.Id, updateCartDetail);

            return Ok(new
            {
                cartdetail.Quantity,
                cartdetail.Total,
                DrinkStockLeft = drink.Quantity + stockChange
            });
        }


        [HttpGet("updateStatus/{id}/{status}")]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            var result = await _cartdetail.UpdateOneAsync(
                c => c.Id == id,
                Builders<CartDetail>.Update.Set(c => c.Status, status));

            if (result.ModifiedCount == 0)
                return NotFound();

            return Ok();
        }

        public class UpdateStatusRequest
        {
            public List<string> Ids { get; set; }
            public string Status { get; set; }
        }

        [HttpPost("updateStatusMultiple")]
        public async Task<IActionResult> UpdateStatusMultiple([FromBody] UpdateStatusRequest request)
        {
            if (request?.Ids == null || request.Ids.Count == 0)
                return BadRequest("Danh sách id trống");

            var filter = Builders<CartDetail>.Filter.In(c => c.Id, request.Ids);
            var update = Builders<CartDetail>.Update.Set(c => c.Status, request.Status);

            var result = await _cartdetail.UpdateManyAsync(filter, update);

            if (result.ModifiedCount == 0)
                return NotFound("Không tìm thấy cartdetail hoặc không có thay đổi");

            return Ok(new { updatedCount = result.ModifiedCount });
        }


    }
}
