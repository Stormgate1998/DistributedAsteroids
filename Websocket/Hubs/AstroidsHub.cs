using System.Text.Json;
using Asteroids.Shared.Services;
using Microsoft.AspNetCore.SignalR;
using Asteroids.Shared.GameObjects;

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

  public async Task SendGameState(GameStateObject state, string connectionId)
  {
    Console.WriteLine("Received game state from hub");

    var client = Clients.Client(connectionId);
    Console.WriteLine($"Socket ship count: {state.ships.Count}");
    await client.SendAsync("ReceiveGameState", state);
  }
}