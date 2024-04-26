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
    _logger.LogInformation($"value is {value}");

    _logger.LogInformation($"Updating data for key: {key}.");
    var response = await _http.PostAsync("/Gateway/Write" +
      $"?key={key}" +
      $"&value={value}", null);
    _logger.LogInformation($"Update response{response.StatusCode.ToString()}");
    _logger.LogInformation($"Response content: {response.Content}");
  }

  public async Task<GameStateObject> GetGameSnapshot(string key)
  {
    _logger.LogInformation($"Getting game snapshot for key: {key}");
    var response = await _http.GetFromJsonAsync<Data>($"/Gateway/StrongGet?key={key}");
    var state = JsonSerializer.Deserialize<GameStateObject>(response.Value);
    _logger.LogInformation($"Game state: {state.state}");
    _logger.LogInformation($"Game ship count: {state.ships.Count}");
    _logger.LogInformation($"Game first player: {state.particpatingUsers.First().Value}");
    return state;
  }
}

public class Data
{
  public string Value { get; set; } = "";
  public int LogIndex { get; set; }
}
