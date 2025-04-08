using Cafeza_BE.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrinkController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drink;

        public DrinkController(MongoDbContext context) {
            _drink = context.Drinks;
        }

        [HttpGet]
        public ActionResult<List<Drink>> GetAll()
        {
            var drinks = _drink.Find(d => true).ToList();
            foreach (var drink in drinks) {

            }
            return Ok(drinks);
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
        public ActionResult<Drink> Create([FromBody] DrinkDTO newDrinkDTO)
        {
            var newDrink = ToEntity(newDrinkDTO);
            _drink.InsertOne(newDrink);
            return Ok(newDrink);
        }
        private Drink ToEntity(DrinkDTO drinkDTO)
        {
            return new Drink
            {
                Sku = drinkDTO.Sku,
                Name = drinkDTO.Name,
                CategoryId = drinkDTO.CategoryId,
                Price = drinkDTO.Price,
                Description = drinkDTO.Description,
                ImagePath = drinkDTO.ImagePath,
                Quantity = drinkDTO.Quantity,
                Status = drinkDTO.Status,
                Size = drinkDTO.Size,
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
    }
}
