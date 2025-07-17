using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    public class ChatController : ControllerBase
    {
        public class ChatRequest
        {
            public string Message { get; set; }
            public string ConversationId { get; set; }

        }

        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Drink> _drink;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<Conversation> _conversation;
        private readonly IMongoCollection<ConversationMembers> _conversationMembers;
        private readonly IMongoCollection<Message> _message;

        public ChatController(IHttpClientFactory httpClientFactory, MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _httpClient = httpClientFactory.CreateClient();
            _drink = context.Drinks;
            _hubContext = hubContext;
            _user = context.Users;
            _conversation = context.Conversations;
            _conversationMembers = context.ConversationMembers;
            _message = context.Messages;
        }




        [HttpPost("createChatAI")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            string apiUrl = "http://127.0.0.1:8000/chat";

            var jsonRequest = JsonSerializer.Serialize(new { text = request.Message });
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, $"Lỗi gọi API chat: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            //var jsonResponse = JsonSerializer.Deserialize<object>(responseContent);
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement.Clone();
            string message = null;
            if (root.TryGetProperty("message", out JsonElement messageElement))
            {
                message = messageElement.GetString();
                //Console.WriteLine("Message: " + message);
            }
            else
            {
                //Console.WriteLine("Không tìm thấy trường 'message'.");
            }

            //if (jsonResponse.intent)

            var messDTO = new MessageDTO
            {
                SenderMemberId = "683aa60a6a13923e9d67b835",
                Content = message,
                ConversationId = request.ConversationId,
                MessageType = "text",
                ParentId = null
            };

            var newMess = ToEntityMessage(messDTO);
            await _message.InsertOneAsync(newMess);
            //var message = await _message.Find(m => m.Id == messDTO.ParentId).FirstOrDefaultAsync();
            var fullName = await _user.Find(u => u.Id == messDTO.SenderMemberId).FirstOrDefaultAsync();
            var responseToSend = new
            {
                request.ConversationId,
                Content = message,
                SenderMemberId = "683aa60a6a13923e9d67b835",
                LastMessage = message,
                MessageType = "text",
                Id = newMess.Id,
                ParentId = newMess.ParentId ?? null,
                createdAt = newMess.CreatedAt,
                fullename = fullName.FullName,
            };

            await _conversation.UpdateOneAsync(
                              co => co.Id == request.ConversationId,
                              Builders<Conversation>.Update.Set(co => co.UpdatedAt, DateTime.UtcNow)
            );


            await _hubContext.Clients.Group(responseToSend.ConversationId).SendAsync("LoadMessagesAL", responseToSend);
            var members = await _conversationMembers
                .Find(cm => cm.ConversationId == request.ConversationId)
                .ToListAsync();

            foreach (var member in members)
            {
                await _hubContext.Clients.Group(member.MemberId).SendAsync("LoadConversation", responseToSend);
            }
            
            return Ok(root);

        }

        public class ChatResAI
        {
            public string ConversationId { get; set; }
            public string Content { get; set; }
            public string SenderMemberId { get; set; }


        }

        private Message ToEntityMessage(MessageDTO dto)
        {
            return new Message
            {
                ConversationId = dto.ConversationId,
                Content = dto.Content,
                SenderMemberId = dto.SenderMemberId,
                MessageType = dto.MessageType,
                CreatedAt = dto.CreatedAt ?? DateTime.Now,
                ParentId = dto.ParentId,
            };
        }

        [HttpPost("createChat")]
        public async Task<IActionResult> CreateChat([FromBody] ChatResAI request)
        {
            //string messageType = GetMessageType(request.Content);
            //var content = string.IsNullOrWhiteSpace(request.Content) ? "👍" : request.Content;

            var messDTO = new MessageDTO
            {
                SenderMemberId = request.SenderMemberId,
                Content = request.Content,
                ConversationId = request.ConversationId,
                MessageType = "text",
                ParentId = null
            };

            var newMess = ToEntityMessage(messDTO);
            await _message.InsertOneAsync(newMess);
            //var message = await _message.Find(m => m.Id == messDTO.ParentId).FirstOrDefaultAsync();
            var fullName = await _user.Find(u => u.Id == messDTO.SenderMemberId).FirstOrDefaultAsync();
            var responseToSend = new
            {
                request.ConversationId,
                request.Content,
                request.SenderMemberId,
                LastMessage = request.Content,
                MessageType = "text",
                Id = newMess.Id,
                ParentId = newMess.ParentId ?? null,
                createdAt = newMess.CreatedAt,
                fullename = fullName.FullName,
            };

            await _conversation.UpdateOneAsync(
                              co => co.Id == request.ConversationId,
                              Builders<Conversation>.Update.Set(co => co.UpdatedAt, DateTime.UtcNow)
            );


            await _hubContext.Clients.Group(responseToSend.ConversationId).SendAsync("LoadMessage", responseToSend);
            var members = await _conversationMembers
                .Find(cm => cm.ConversationId == request.ConversationId)
                .ToListAsync();

            foreach (var member in members)
            {
                await _hubContext.Clients.Group(member.MemberId).SendAsync("LoadConversation", responseToSend);
            }

            return Ok();
        }



    }
}
