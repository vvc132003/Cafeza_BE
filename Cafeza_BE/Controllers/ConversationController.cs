using System.Text.RegularExpressions;
using Cafeza_BE.DB;
using Cafeza_BE.Hub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
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

               

                var lastMessage = messages.LastOrDefault()?.Content ?? "";

                results.Add(new
                {
                    conversationId = conversation.Id,
                    name = user?.FullName ?? "Không rõ",
                    avatar = "https://i.pravatar.cc/150?img=1",
                    lastMessage,
                    messageType = messages.LastOrDefault()?.MessageType,
                });
            }

            return Ok(results);
        }

        [HttpGet("getMessages/{conversationId}/{userId}")]
        public async Task<IActionResult> GetMessages(string conversationId, string userId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var results = new List<object>();

            var conversation = await _conversation.Find(c => c.Id == conversationId).FirstOrDefaultAsync();
            if (conversation == null)
            {
                return NotFound("Conversation not found");
            }

            var members = await _conversationMembers
                          .Find(m => m.ConversationId == conversation.Id)
                          .ToListAsync();

            var otherMember = members.FirstOrDefault(m => m.MemberId != userId);

            var user = await _user.Find(u => u.Id == otherMember.MemberId).FirstOrDefaultAsync();

            // Tính skip và limit để phân trang
            int skip = (page - 1) * pageSize;

            var messages = await _message
                            .Find(m => m.ConversationId == conversation.Id)
                            .SortByDescending(m => m.CreatedAt) // Lấy tin mới nhất trước (để pagination dễ lấy page 1 là tin mới nhất)
                            .Skip(skip)
                            .Limit(pageSize)
                            .ToListAsync();

            // Vì sort descending, nên đảo lại cho hiển thị theo thứ tự thời gian tăng dần
            messages.Reverse();

            var formattedMessages = messages.Select(m => new
            {
                text = m.Content,
                senderMemberId = m.SenderMemberId,
                messageType = m.MessageType,
                parentId = m.ParentId,
                id = m.Id,
            }).ToList();

            results.Add(new
            {
                conversationId = conversation.Id,
                name = user?.FullName ?? "Không rõ",
                avatar = "https://i.pravatar.cc/150?img=1",
                messages = formattedMessages,
                page = page,
                pageSize = pageSize
            });

            return Ok(results);
        }


        //[HttpGet("getMessages/{conversationId}/{userId}")]
        //public async Task<IActionResult> GetMessages(string conversationId, string userId)
        //{
        //    var results = new List<object>(); // lấy dwxl iệu chat

        //    //var conversations = await _conversation.Find(c => c.Id == conversationId).ToListAsync();
        //    var conversation = await _conversation.Find(c => c.Id == conversationId).FirstOrDefaultAsync();

        //    //foreach (var conversation in conversations)
        //    //{
        //        var members = await _conversationMembers
        //                      .Find(m => m.ConversationId == conversation.Id)
        //                      .ToListAsync();
        //        var otherMember = members.FirstOrDefault(m => m.MemberId != userId);

        //        var user = await _user.Find(u => u.Id == otherMember.MemberId).FirstOrDefaultAsync();

        //        var messages = await _message
        //                        .Find(m => m.ConversationId == conversation.Id)
        //                        .SortBy(m => m.CreatedAt)
        //                        .ToListAsync();

        //        var formattedMessages = messages.Select(m => new
        //        {
        //            text = m.Content,
        //            senderMemberId = m.SenderMemberId,
        //            messageType = m.MessageType,
        //            parentId = m.ParentId,
        //            id = m.Id,
        //        }).ToList();

        //    results.Add(new
        //         {
        //             conversationId = conversation.Id,
        //             name = user?.FullName ?? "Không rõ",
        //             avatar = "https://i.pravatar.cc/150?img=1",
        //             messages = formattedMessages,
        //         });
        //    //}

        //    return Ok(results);
        //}

        public class ChatRes {
            public string ConversationId { get; set; }
            public string Content { get; set; }
            public string SenderMemberId { get; set; }


        }



        [HttpPost("createChat")]
        public async Task<IActionResult> createChat([FromBody] ChatRes request)
        {
            string messageType = GetMessageType(request.Content);

            var messDTO = new MessageDTO
            {
                SenderMemberId = request.SenderMemberId,
                Content = request.Content,
                ConversationId = request.ConversationId,
                MessageType = messageType,
                ParentId  = null
            };

            var newMess = ToEntityMessage(messDTO);
            await _message.InsertOneAsync(newMess);

            var responseToSend = new
            {
                request.ConversationId,
                request.Content,
                request.SenderMemberId,
                LastMessage = request.Content,
                MessageType = messageType,
                Id = newMess.Id,
                ParentId = newMess.ParentId ?? null,
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

        private string GetMessageType(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "text";

            // ✅ Nếu là video base64
            if (content.StartsWith("data:video/", StringComparison.OrdinalIgnoreCase))
                return "video";

            // ✅ Nếu là ảnh base64
            if (content.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                return "image";

            // ✅ Nếu là URL hợp lệ
            if (Uri.IsWellFormedUriString(content, UriKind.Absolute))
            {
                string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                string[] videoExtensions = { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };
                string lowerContent = content.ToLower();

                if (imageExtensions.Any(ext => lowerContent.EndsWith(ext)))
                    return "image";

                if (videoExtensions.Any(ext => lowerContent.EndsWith(ext)))
                    return "video";

                // ✅ Nếu là YouTube URL
                if (lowerContent.Contains("youtube.com/watch") || lowerContent.Contains("youtu.be/"))
                    return "video";

                return "link";
            }

            return "text";
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
        public class TypingRequest
        {
            public string UserId { get; set; }
            public string ConversationId { get; set; }
            public bool IsTyping { get; set; }
        }

        [HttpPost("log-typing")]
        public async Task<IActionResult> LogTyping([FromBody] TypingRequest request)
        {
            var members = await _conversationMembers
                 .Find(cm => cm.ConversationId == request.ConversationId)
                 .ToListAsync();

            foreach (var member in members)
            {
                await _hubContext.Clients.Group(member.MemberId).SendAsync("ReceiveTypingStatus", request);
            }
            return Ok();
        }
        public class ReplyMessageRes
        {
            public string ConversationId { get; set; }
            public string Content { get; set; }
            public string SenderMemberId { get; set; }
            public string ParentId { get; set; }

        }

        [HttpPost("replyMessage")]
        public async Task<IActionResult> ReplyMessage([FromBody] ReplyMessageRes request)
        {
            string messageType = GetMessageType(request.Content);

            
            var messDTO = new MessageDTO
            {
                SenderMemberId = request.SenderMemberId,
                Content = request.Content,
                ConversationId = request.ConversationId,
                MessageType = messageType,
                ParentId = request.ParentId,
            };

            var newMess = ToEntityMessage(messDTO);
            await _message.InsertOneAsync(newMess);

            var responseToSend = new
            {
                request.ConversationId,
                request.Content,
                request.SenderMemberId,
                LastMessage = request.Content,
                MessageType = messageType,
                ParentId = request.ParentId,
                Id = newMess.Id,
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
