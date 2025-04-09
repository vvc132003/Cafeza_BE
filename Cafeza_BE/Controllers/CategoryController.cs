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
            return Ok(new { message = "Thêm thành công", data = entity });
        }


        private Category ToEntity(CategoryDTO dto)
        {
            return new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder,
                ImageUrl = dto.ImageUrl,
                Tags = dto.Tags,
                Slug = dto.Slug,
                ShowOnHome = dto.ShowOnHome,
                Icon = dto.Icon,
                ColorCode = dto.ColorCode,
                ViewCount = dto.ViewCount,
                ParentId = dto.ParentId,
                Notes = dto.Notes,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

    }
}
