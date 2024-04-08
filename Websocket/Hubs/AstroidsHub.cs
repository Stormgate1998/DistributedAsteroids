using System.Text.Json;
using Asteroids.Shared.Services;
using Microsoft.AspNetCore.SignalR;

namespace Websocket.Hubs;

public class AsteroidsHub : Hub
{
  public async Task SendLobbyList(List<string> message, string connectionId)
  {
    Console.WriteLine("Received list of lobbies in hub.");
    Console.WriteLine(JsonSerializer.Serialize(message));
    var client = Clients.Client(connectionId);

    await client.SendAsync("ReceiveLobbiesList", message);
  }

  public async Task SendClientState(ClientState state, string connectionId)
  {
    Console.WriteLine("Received client state in hub.");
    var client = Clients.Client(connectionId);

    await client.SendAsync("ReceiveClientState", state);
  }
}