using Cafeza_BE.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model;
using MongoDB.Driver;

namespace Cafeza_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMongoCollection<Drink> _drink;
        private readonly IMongoCollection<Category> _category;

        public CategoryController(MongoDbContext context)
        {
            _drink = context.Drinks;
            _category = context.Categorys;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _category.Find(_ => true).ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi server", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrEmpty(categoryDto.Name))
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            var entity = ToEntity(categoryDto);
            await _category.InsertOneAsync(entity);
            return Ok(entity);
        }


        private Category ToEntity(CategoryDTO dto)
        {
            return new Category
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                DisplayName = dto.DisplayName,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                Slug = dto.Slug,
                ShowOnHome = dto.ShowOnHome,
                Icon = dto.Icon,
                ColorCode = dto.ColorCode,
                ParentId = dto.ParentId,
                Notes = dto.Notes,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        [HttpPut]
        public IActionResult Update([FromBody] Category updateCategory)
        {
            //var drink = _drink.Find(d => d.Id == updateDrinkDTO.Id).FirstOrDefault();
            //if (drink == null)
            //    return NotFound();
            //var updateDrink = ToEntity(updateDrinkDTO);
            _category.ReplaceOne(d => d.Id == updateCategory.Id, updateCategory);
            return Ok(updateCategory);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var result = _category.DeleteOne(d => d.Id == id);
            if (result.DeletedCount == 0)
                return NotFound();
            return NoContent();
        }

    }
}
