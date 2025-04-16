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
    public class TableController : ControllerBase
    {
        private readonly IMongoCollection<Table> _table;
        private readonly IHubContext<SignalRHub> _hubContext;

        public TableController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _table = context.Tables;
            _hubContext = hubContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
                var tables = await _table.Find(_ => true).ToListAsync();
                return Ok(tables);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] TableDTO tableDTO)
        {
            var entity = ToEntity(tableDTO);
            await _table.InsertOneAsync(entity);
            await _hubContext.Clients.All.SendAsync("loadTable", entity);
            return Ok(entity);
        }
        private Table ToEntity(TableDTO dto)
        {
            return new Table
            {
                TableName = dto.TableName,
                Capacity = dto.Capacity,
                Status = dto.Status,
                Type = dto.Type,
                Location = dto.Location,
                Note = dto.Note,
                IsDeleted = dto.IsDeleted ?? false,
                IsReservable = dto.IsReservable,
                Tags = dto.Tags,
                ParentId = dto.ParentId,
            };
        }


    }
}
