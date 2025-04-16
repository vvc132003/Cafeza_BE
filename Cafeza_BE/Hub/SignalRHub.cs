
namespace Cafeza_BE.Hub
{
    using Microsoft.AspNetCore.SignalR;
    public class SignalRHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
