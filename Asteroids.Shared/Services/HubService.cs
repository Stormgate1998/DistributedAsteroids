using Microsoft.AspNetCore.SignalR.Client;
using Asteroids.Shared.GameObjects;
using Microsoft.Extensions.Logging;

namespace Asteroids.Shared.Services;

public class HubService : IHubService
{
  private readonly HubConnection _connectionId;
  private readonly ILogger<HubService> _logger;
  // private readonly string _receiverConnectionId;

  public HubService(ILogger<HubService> logger)
  {
    // _receiverConnectionId = connectionId;
    _logger = logger;

    _connectionId = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr:8080/asteroidsHub")
      .WithAutomaticReconnect()
      .Build();
  }

  public async Task EnsureHubConnection()
  {
    if (_connectionId.State == HubConnectionState.Disconnected)
    {
      _logger.LogInformation("Establishing connection to websocket hub.");
      
      await _connectionId.StartAsync().ContinueWith(task =>
    {
      if (task.IsFaulted)
      {
        _logger.LogInformation($"Error connecting to SignalR hub: {task.Exception.GetBaseException().Message}");
      }
      else
      {
        _logger.LogInformation("SignalR connection established.");
      }
    });
    }
  }

  public async Task SendClientState(string connectionId, ClientState state)
  {
    await EnsureHubConnection();

    _logger.LogInformation($"Sending client state to hub for connection ID: {connectionId}.");
    await _connectionId.SendAsync("SendClientState", state, connectionId);
  }

  public async Task SendGameSnapshot(string connectionId, GameStateObject game)
  {
    await EnsureHubConnection();

    _logger.LogInformation($"Sending game snapshot of {game.LobbyName} to hub for connection ID: {connectionId}.");
    await _connectionId.SendAsync("SendGameState", game, connectionId);
  }

  public async Task StopAsync() => await _connectionId.StopAsync();

  public async Task SendLobbyList(List<string> lobbyList, string connectionId)
  {
    await EnsureHubConnection();

    _logger.LogInformation($"Sending list of lobbies to hub for connection ID: {connectionId}.");
    await _connectionId.SendAsync("SendLobbyList", lobbyList, connectionId);
  }

  public async Task SendCountDownNumber(int number, string connectionId)
  {
    await EnsureHubConnection();

    _logger.LogInformation($"Sending countdown number to hub: {number} for connection ID: {connectionId}");
    await _connectionId.SendAsync("SendCountdown", number, connectionId);
  }
}
