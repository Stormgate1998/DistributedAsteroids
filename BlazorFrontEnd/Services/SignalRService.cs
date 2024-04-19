using Asteroids.Shared.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Asteroids.Shared.GameObjects;

namespace BlazorFrontEnd.Services;

public class SignalRService
{
  public event Action<List<string>>? NewLobbyList;
  public event Action<ClientState>? NewClientState;
  public event Action<GameStateObject>? NewGameState;

  public event Action<int>? NewCountDown;
  private readonly ILogger _logger;
  private readonly HubConnection HubConnection;

  public SignalRService(ILogger<SignalRService> logger)
  {
    _logger = logger;

    HubConnection = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr:8080/asteroidsHub")
      .Build();


    HubConnection.On<List<string>>("ReceiveLobbiesList", (lobbies) =>
    {
      // Console.WriteLine("Received lobby list for blazor.");
      _logger.LogInformation("Received lobby list for client.");

      NewLobbyList?.Invoke(lobbies);
    });

    HubConnection.On<ClientState>("ReceiveClientState", (state) =>
    {
      // Console.WriteLine("Received client state for blazor.");
      _logger.LogInformation("Received client state for client.");
      NewClientState?.Invoke(state);
    });

    HubConnection.On<GameStateObject>("ReceiveGameState", (state) =>
    {
      // Console.WriteLine("Received game state for blazor.");
      Console.WriteLine($"Blazor ship count: {state.ships.Count}");
      _logger.LogInformation("Received game state for client.");
      NewGameState?.Invoke(state);
    });

    HubConnection.On<int>("ReceiveCountdown", (state) =>
    {
      // Console.WriteLine("Invoking countdown event.");
      _logger.LogInformation("Invoking countdown event.");
      NewCountDown?.Invoke(state);
    });
  }

  public async Task EnsureStartedAsync()
  {
    if (HubConnection.State != HubConnectionState.Disconnected)
    {
      return;
    }

    // Console.WriteLine("Establishing connection to websocket hub.");
    _logger.LogInformation("Establishing connection to websocket hub.");
    await HubConnection.StartAsync().ContinueWith(task =>
    {
      if (task.IsFaulted)
      {
        // Console.WriteLine($"Error connecting to SignalR hub: {task.Exception.GetBaseException().Message}");
        _logger.LogInformation($"Error connecting to SignalR hub: {task.Exception.GetBaseException().Message}");
      }
      else
      {
        // Console.WriteLine("SignalR connection established");
        _logger.LogInformation("SignalR connection established");
      }
    });
  }

  public async Task<string> GetConnectionId()
  {
    await EnsureStartedAsync();

    return HubConnection.ConnectionId ?? throw new NullReferenceException("Could not get connection ID. It is null.");
  }
}