
namespace Cafeza_BE.Hub
{
    using Microsoft.AspNetCore.SignalR;
    using Model;

    public class SignalRHub : Hub
    {
        //public async Task SendMessage(string user, string message)
        //{
        //    await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}

        public async Task LoadOrderId(string orderId, OrderDetailDTO orderDetailDTO)
        {
            await Clients.Group(orderId).SendAsync("ReceiveorderDetailDTO", orderDetailDTO);
        }

        public async Task JoinOrderDetailOrderId(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }

        public async Task LeaveGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }
    }
}
