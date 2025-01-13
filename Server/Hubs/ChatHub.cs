using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message, string avatarImagePath)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, avatarImagePath);
        }
    }
}
