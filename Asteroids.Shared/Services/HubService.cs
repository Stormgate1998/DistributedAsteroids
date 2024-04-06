using Microsoft.AspNetCore.SignalR.Client;

namespace Asteroids.Shared.Services;

public class HubService : IHubService
{
  private readonly HubConnection _connectionId;
  private readonly string _receiverConnectionId;

  public HubService(string connectionId)
  {
    _receiverConnectionId = connectionId;
    _connectionId = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr/asteroidsHub")
      .Build();

    Console.WriteLine("Starting connection to websocket");
    _connectionId.StartAsync().ContinueWith(task =>
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

  // public async Task StartAsync()
  // {
  //   await _connectionId.StartAsync();
  // }

  public async Task StopAsync() => await _connectionId.StopAsync();

  public async Task SendLobbyList(List<string> lobbyList)
  {
    await _connectionId.SendAsync("SendLobbyList", lobbyList, _receiverConnectionId);
  }
}
