using Asteroids.Shared.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Asteroids.Shared.GameObjects;

namespace BlazorFrontEnd.Services;

public class SignalRService
{
  public event Action<List<string>>? NewLobbyList;
  public event Action<ClientState>? NewClientState;
  public event Action<GameStateObject>? NewGameState;
  private readonly HubConnection HubConnection;

  public SignalRService()
  {
    HubConnection = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr:8080/asteroidsHub")
      .Build();


    HubConnection.On<List<string>>("ReceiveLobbiesList", (lobbies) =>
    {
      Console.WriteLine("Received lobby list for blazor.");
      NewLobbyList?.Invoke(lobbies);
    });

    HubConnection.On<ClientState>("ReceiveClientState", (state) =>
    {
      Console.WriteLine("Received client state for blazor.");
      NewClientState?.Invoke(state);
    });
    
    HubConnection.On<GameStateObject>("ReceiveGameState", (state) =>
    {
      Console.WriteLine("Received game state for blazor.");
      NewGameState?.Invoke(state);
    });
  }

  public async Task EnsureStartedAsync()
  {
    if (HubConnection.State != HubConnectionState.Disconnected)
    {
      return;
    }

    Console.WriteLine("Establishing connection to websocket hub.");
    await HubConnection.StartAsync().ContinueWith(task =>
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

  public async Task<string> GetConnectionId()
  {
    await EnsureStartedAsync();

    return HubConnection.ConnectionId ?? throw new NullReferenceException("Could not get connection ID. It is null.");
  }
}