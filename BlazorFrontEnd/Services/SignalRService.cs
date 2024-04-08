using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorFrontEnd.Services;

public class SignalRService
{
  public readonly HubConnection HubConnection;
  public List<Action<List<string>>> LobbyActionList = [];

  public SignalRService()
  {
    HubConnection = new HubConnectionBuilder()
      .WithUrl("http://je-asteroids-signalr:8080/asteroidsHub")
      .Build();


    HubConnection.On<List<string>>("ReceiveLobbiesList", (lobbies) =>
    {
      foreach (var action in LobbyActionList)
      {
        action(lobbies);
      }

    });


    HubConnection.StartAsync();
  }

  public List<string> HandleLobbiesListRequest(List<string> lobbies)
  {
    return lobbies;
  }

  public async Task ReceiveLobbiesList(Action<List<string>> lobbyListFunction)
  {
    LobbyActionList.Add(lobbyListFunction);

  }
}