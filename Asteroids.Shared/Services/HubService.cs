using Microsoft.AspNetCore.SignalR.Client;
using Asteroids.Shared.GameObjects;

namespace Asteroids.Shared.Services;

public class HubService : IHubService
{
  private readonly HubConnection _connectionId;
  // private readonly string _receiverConnectionId;

  public HubService()
  {
    // _receiverConnectionId = connectionId;
    _connectionId = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr:8080/asteroidsHub")
      .WithAutomaticReconnect()
      .Build();
  }

  public async Task EnsureHubConnection()
  {
    if (_connectionId.State == HubConnectionState.Disconnected)
    {
      Console.WriteLine("Establishing connection to websocket hub.");
      await _connectionId.StartAsync().ContinueWith(task =>
    {
      if (task.IsFaulted)
      {
        Console.WriteLine($"Error connecting to SignalR hub: {task.Exception.GetBaseException().Message}");
      }
      else
      {
        Console.WriteLine("SignalR connection established");
      }
    });
    }
  }

  public async Task SendClientState(string connectionId, ClientState state)
  {
    await EnsureHubConnection();

    Console.WriteLine("Sending client state to hub.");
    await _connectionId.SendAsync("SendClientState", state, connectionId);
  }

  public async Task SendGameSnapshot(string connectionId, GameStateObject game)
  {
    await EnsureHubConnection();
    Console.WriteLine("sending current game state");
    Console.WriteLine($"Ship count: {game.ships.Count}");
    await _connectionId.SendAsync("SendGameState", game, connectionId);
  }

  public async Task StopAsync() => await _connectionId.StopAsync();

  public async Task SendLobbyList(List<string> lobbyList, string connectionId)
  {
    await EnsureHubConnection();

    Console.WriteLine("Sending list of lobbies to hub.");
    await _connectionId.SendAsync("SendLobbyList", lobbyList, connectionId);
  }

  public async Task SendCountDownNumber(int number, string connectionId)
  {
    await EnsureHubConnection();

    Console.WriteLine($"Sending countdown number to hub: {number}");
    await _connectionId.SendAsync("SendCountdown", number, connectionId);

  }
}
