namespace Asteroids.Shared.Services;

public interface IHubService
{
  // public Task StartAsync();
  public Task StopAsync();
  public Task SendLobbyList(List<string> lobbyList);
}