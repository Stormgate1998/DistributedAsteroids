using System.Net.Http.Json;
using System.Text.Json;
using Asteroids.Shared.GameObjects;
using Microsoft.Extensions.Logging;

namespace Asteroids.Shared.Services;

public class RaftService : IRaftService
{
  private HttpClient _http;
  private readonly ILogger<RaftService> _logger;

  public RaftService(HttpClient http, ILogger<RaftService> logger)
  {
    _http = http;
    _logger = logger;
  }

  public async Task StoreGameSnapshot(string key, GameStateObject snapshot)
  {
    string value = JsonSerializer.Serialize(snapshot);

    _logger.LogInformation($"Updating data for key: {key}.");
    await _http.PostAsync("/Gateway/Write" +
      $"?key={key}" +
      $"&value={value}", null);
  }

  public async Task<GameStateObject> GetGameSnapshot(string key)
  {
    var response = await _http.GetFromJsonAsync<Data>($"/Gateway/StrongGet?key={key}");

    var state = JsonSerializer.Deserialize<GameStateObject>(response.Value);

    return state;
  }
}

public class Data
{
  public string Value { get; set; } = "";
  public int LogIndex { get; set; }
}
