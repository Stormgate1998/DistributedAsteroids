namespace Asteroids.Shared.Services;

public interface IHubService
{
  // public Task StartAsync();
  public Task EnsureHubConnection();
  
  public Task SendClientState(string connectionId, ClientState state);

  public Task StopAsync();
  public Task SendLobbyList(List<string> lobbyList, string connectionId);
}

public enum ClientState
{
  NoLobby,
  InLobby
}