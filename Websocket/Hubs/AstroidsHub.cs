using Microsoft.AspNetCore.SignalR;

namespace Websocket.Hubs;

public class AsteroidsHub : Hub
{
  public async Task SendLobbyList(List<string> message, string connectionId)
  {
    Console.WriteLine("Received list of lobbies in hub.");
    var client = Clients.Client(connectionId);

    await client.SendAsync("ReceiveLobbiesList", message);
  }
}