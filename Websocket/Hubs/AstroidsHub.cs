using Microsoft.AspNetCore.SignalR;

namespace Websocket.Hubs;

public class AsteroidsHub : Hub
{
  public async Task SendMessage(string user, string message)
  {
    await Clients.All.SendAsync("ReceiveMessage", user, message);
  }
}