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
    public class ConversationController : ControllerBase
    {
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<Conversation> _conversation;
        private readonly IMongoCollection<ConversationMembers> _conversationMembers;
        private readonly IMongoCollection<Message> _message;


        public ConversationController(MongoDbContext context, IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
            _user = context.Users;
            _conversation = context.Conversations;
            _conversationMembers = context.ConversationMembers;
            _message = context.Messages;
        }

        public class CreateConverstationRequest
        {
            public string role { get; set; } // role
            public string userId2 { get; set; } // custoemr or login
        }

        
        [HttpPost("createConverstation")]
        public async Task<IActionResult> CreateConverstation([FromBody] CreateConverstationRequest request)
        {
            //var employeeDetail = await _
            //var user = await _user.Find(u => u.Role ===)
            // 1. Tìm tất cả các cuộc hội thoại 1-1

            var oneOnOneConversations = await _conversation
              .Find(c => c.ConversationType == "1-1")
              .ToListAsync();
            // 2. Lặp qua từng cuộc hội thoại để kiểm tra thành viên
            //var results = new List<object>(); // lấy dwxl iệu chat

            foreach (var conversation in oneOnOneConversations)
            {
                // Lấy danh sách các thành viên trong cuộc hội thoại hiện tại
                var members = await _conversationMembers
                .Find(m => m.ConversationId == conversation.Id)
                .ToListAsync();
                // Lấy danh sách ID thành viên
                var memberIds = members.Select(m => m.MemberId).ToList();
               

                // 3. Kiểm tra xem cuộc hội thoại có đúng 2 người, và đó là userId1 & userId2
                if (memberIds.Count == 2 && memberIds.Contains("682ec9658b48754bc03d9c5f") && memberIds.Contains(request.userId2))
                { 
                    //// Lấy người còn lại(không phải currentUser)
                    //var otherMember = members.FirstOrDefault(m => m.MemberId != request.userId2);
                    //if (otherMember == null) continue;
                    //// lấy thông tin người login
                    //var user = await _user.Find(u => u.Id == otherMember.MemberId).FirstOrDefaultAsync();
                    ////  Lấy tất cả message trong conversation
                    //var messages = await _message
                    //    .Find(m => m.ConversationId == conversation.Id)
                    //    .SortBy(m => m.CreatedAt)
                    //    .ToListAsync();

                    //// Chuyển đổi messages sang dạng bạn cần (fromSelf)
                    //var formattedMessages = messages.Select(m => new
                    //{
                    //    text = m.Content,
                    //    fromSelf = m.SenderMemberId == request.userId2
                    //}).ToList();
                    
                    //// Lấy message cuối cùng để hiển thị lastMessage
                    //var lastMessage = messages.LastOrDefault()?.Content ?? "";
                    //// 8. Tạo đối tượng kết quả
                    //results.Add(new
                    //{
                    //    name = user?.FullName ?? "Không rõ",
                    //    avatar = "https://i.pravatar.cc/150?img=1",
                    //    lastMessage,
                    //    messages = formattedMessages
                    //});
                    //// Nếu tìm thấy thì trả về cuộc hội thoại đó
                    return Ok(true);
                }
            }
            // 4. Nếu không tìm thấy, tạo mới một cuộc hội thoại 1-1
            if (request.role != "customer")
            {
                return BadRequest("Chỉ khách hàng mới được tạo cuộc hội thoại mới.");
            }

            var newConversation = new Conversation
            {
                Title = null,                         // Cuộc hội thoại 1-1 không cần tiêu đề
                ConversationType = "1-1",             // Kiểu cuộc hội thoại là "1-1"
                CreatedAt = DateTime.UtcNow,          // Ghi lại thời điểm tạo
                UpdatedAt = DateTime.UtcNow           // Ghi lại thời điểm cập nhật
            };

            // Lưu cuộc hội thoại mới vào cơ sở dữ liệu
            await _conversation.InsertOneAsync(newConversation);

            // 5. Tạo thông tin thành viên A (người khởi tạo hoặc đối tượng tham gia)
            var memberA = new ConversationMembers
            {
                ConversationId = newConversation.Id,
                MemberId = "682ec9658b48754bc03d9c5f",
                MemberType = "employee",              // Loại người dùng (tuỳ logic ứng dụng)
                CreatedAt = DateTime.UtcNow
            };

            // 6. Tạo thông tin thành viên B
            var memberB = new ConversationMembers
            {
                ConversationId = newConversation.Id,
                MemberId = request.userId2,
                MemberType = "customer",              // Loại người dùng (tuỳ logic ứng dụng)
                CreatedAt = DateTime.UtcNow
            };

            // 7. Lưu cả 2 thành viên vào bảng ConversationMembers
            await _conversationMembers.InsertManyAsync(new[] { memberA, memberB });


//            var messages = new List<Message>
//{
//    new Message
//    {
//        ConversationId = newConversation.Id,
//        SenderMemberId = "682ec9658b48754bc03d9c5f",
//        MessageType = "text",
//        Content = "Xin chào, bạn cầsfdgvn hỗ trợ gì?",
//        CreatedAt = DateTime.UtcNow.AddMinutes(-15)
//    },
//    new Message
//    {
//        ConversationId = newConversation.Id,
//        SenderMemberId = "682ecf6b306e370026dbff62",
//        MessageType = "text",
//        Content = "Tôi muốn đặsdvkhách sạn.",
//        CreatedAt = DateTime.UtcNow.AddMinutes(-10)
//    },
//    new Message
//    {
//        ConversationId = newConversation.Id,
//        SenderMemberId = "682ec9658b48754bc03d9c5f",
//        MessageType = "text",
//        Content = "Bạn muốn đưef phòng nào?",
//        CreatedAt = DateTime.UtcNow.AddMinutes(-5)
//    },
//    new Message
//    {
//        ConversationId = newConversation.Id,
//        SenderMemberId = "682ecf6b306e370026dbff62",
//        MessageType = "text",
//        Content = "à vấn.",
//        CreatedAt = DateTime.UtcNow.AddMinutes(-3)
//    }
//};

//await _message.InsertManyAsync(messages);


            // 8. Trả về cuộc hội thoại vừa tạo
            return Ok(newConversation);
        }


        [HttpGet("getConversations/{userId}")]
        public async Task<IActionResult> GetConversations(string userId)
        {
            var results = new List<object>(); // lấy dwxl iệu chat
            var conversationMembers = await _conversationMembers.Find(cm => cm.MemberId == userId).ToListAsync();
            if (conversationMembers.Count == 0)
            {
                // Trả về danh sách rỗng
                return Ok(results);
            }
            var conversationIds = conversationMembers.Select(cm => cm.ConversationId).ToList();
            var conversations = await _conversation
                                   .Find(c => conversationIds.Contains(c.Id))
                                   .SortByDescending(c => c.UpdatedAt)
                                   .ToListAsync();
            foreach (var conversation in conversations)
            {
                var members = await _conversationMembers
                              .Find(m => m.ConversationId == conversation.Id)
                              .ToListAsync();
                var otherMember = members.FirstOrDefault(m => m.MemberId != userId);

                var user = await _user.Find(u => u.Id == otherMember.MemberId).FirstOrDefaultAsync();

                var messages = await _message
                                .Find(m => m.ConversationId == conversation.Id)
                                .SortBy(m => m.CreatedAt)
                                .ToListAsync();

                var formattedMessages = messages.Select(m => new
                {
                    text = m.Content,
                    fromSelf = m.SenderMemberId == userId
                }).ToList();

                var lastMessage = messages.LastOrDefault()?.Content ?? "";

                 results.Add(new
                 {
                     conversationId = conversation.Id,
                     name = user?.FullName ?? "Không rõ",
                     avatar = "https://i.pravatar.cc/150?img=1",
                     lastMessage,
                     messages = formattedMessages
                 });
            }

            return Ok(results);
        }

    }
}
