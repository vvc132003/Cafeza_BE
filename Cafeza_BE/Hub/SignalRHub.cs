﻿
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

        //public async Task LoadOrderId(string orderId, OrderDetailDTO orderDetailDTO)
        //{
        //    await Clients.Group(orderId).SendAsync("ReceiveorderDetailDTO", orderDetailDTO);
        //}

        public async Task JoinOrderDetailOrderId(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, orderId);
        }

        public async Task LeaveGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, orderId);
        }

        //// chat
        ///
        public async Task JoinGropsChat(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task JoinGropsChatConversation(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        //public async Task SendTypingStatus(string conversationId, string userId)
        //{
        //    await Clients.OthersInGroup(conversationId)
        //                 .SendAsync("ReceiveTypingStatus", userId);
        //}

    }
}
