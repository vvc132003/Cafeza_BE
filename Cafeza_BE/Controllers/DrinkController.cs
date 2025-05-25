using System.Threading.Tasks;
using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;
using SharpCompress.Common;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrinkController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<Category> _category;
        private readonly IHubContext<SignalRHub> _hubContext;


        public DrinkController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _drink = context.Drinks;
            _category = context.Categorys;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult<List<Drink>> GetAll()
        {
            var drinks = _drink.Find(d => true).ToList();
            //foreach (var drink in drinks) {

            //}
            return Ok(drinks);
        }

        [HttpGet("drink-list")]
        public async Task<ActionResult<List<object>>> DrinkList()
        {
            var categorys =  await _category.Find(c=>c.ShowOnHome == true).ToListAsync();
            if (categorys == null || categorys.Count == 0)
            {
                return Ok(null);
            }
            var data = new List<object>();
            foreach (var category in categorys) {
                var drinks = await _drink.Find(d => d.CategoryId == category.Id && d.Status != "discontinued").ToListAsync();
                data.Add(new
                {
                    Category = category.DisplayName,  
                    Drinks = drinks       
                });
            }
            return Ok(data);
        }

        private class ExtenDrinks
        {
            public string? Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string? CategoryId { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string? ImagePath { get; set; }
            public int Quantity { get; set; }
            public string Status { get; set; }  // "available", "out_of_stock", "discontinued"
            public string Size { get; set; }    // "small", "medium", "large"
        }

        //public class Drinkand

        [HttpGet("{id:length(24)}")]
        public ActionResult<Drink> GetById(string id)
        {
            var drink = _drink.Find(d => d.Id == id).FirstOrDefault();
            if (drink == null)
                return NotFound();
            return drink;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DrinkDTO newDrinkDTO)
        {
            var newDrink = ToEntity(newDrinkDTO);
            _drink.InsertOne(newDrink);
            await _hubContext.Clients.All.SendAsync("loadDrink", newDrink);

            return Ok(newDrink);
        }
        private Drink ToEntity(DrinkDTO drinkDTO)
        {
            return new Drink
            {
                Sku = drinkDTO.Sku,
                Slug = drinkDTO.Slug,
                Name = drinkDTO.Name,
                CategoryId = drinkDTO.CategoryId,
                Price = drinkDTO.Price,
                Description = drinkDTO.Description,
                ImagePath = drinkDTO.ImagePath,
                Quantity = drinkDTO.Quantity,
                Status = drinkDTO.Status,
                Size = drinkDTO.Size,
                ViewCount= drinkDTO.ViewCount,
                CreatedAt = drinkDTO.CreatedAt,
                UpdatedAt = drinkDTO.UpdatedAt
            };
        }

        [HttpPut]
        public IActionResult Update([FromBody] Drink updateDrink)
        {
            //var drink = _drink.Find(d => d.Id == updateDrinkDTO.Id).FirstOrDefault();
            //if (drink == null)
            //    return NotFound();
            //var updateDrink = ToEntity(updateDrinkDTO);
            _drink.ReplaceOne(d => d.Id == updateDrink.Id, updateDrink);
            return Ok(updateDrink);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var result = _drink.DeleteOne(d => d.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();
            return NoContent();
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomDrinks([FromQuery] int count = 6)
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("Status", "available")),
                new BsonDocument("$sample", new BsonDocument("size", count))
            };

            var randomDrinks = await _drink.Aggregate<Drink>(pipeline).ToListAsync();

            return Ok(randomDrinks);
        }
    }
}
