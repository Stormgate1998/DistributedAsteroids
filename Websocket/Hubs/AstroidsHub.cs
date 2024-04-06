using Microsoft.AspNetCore.SignalR;

namespace Websocket.Hubs;

public class AsteroidsHub : Hub
{
  public async Task SendLobbyList(List<string> message, string connectionId)
  {
    var client = Clients.Client(connectionId);

    await client.SendAsync("<method-name>", message);
  }
}