using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorFrontEnd.Services;

public class SignalRService
{
  public readonly HubConnection HubConnection;

  public SignalRService()
  {
    HubConnection = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr/asteroidsHub")
      .Build();

    // _hubConnection.On<List<string>>("ReceiveLobbiesList", (lobbies) =>
    // {

    // });

    HubConnection.StartAsync();
  }

  public List<string> HandleLobbiesListRequest(List<string> lobbies)
  {
    return lobbies;
  }

  public async Task ReceiveLobbiesList(Action<List<string>> lobbyListFunction){
    HubConnection.On<List<string>>("ReceiveLobbiesList", (lobbies) =>
    {
      lobbyListFunction(lobbies);
    

    });
  }
}