using Asteroids.Shared.GameObjects;

namespace Asteroids.Shared.Services;

public interface IRaftService
{
  public Task StoreGameSnapshot(string key, GameStateObject snapshot);

  public Task<GameStateObject> GetGameSnapshot(string key);

  public Task StoreLobbyList(List<string> List);
  public Task<List<string>> GetLobbyList();
}
