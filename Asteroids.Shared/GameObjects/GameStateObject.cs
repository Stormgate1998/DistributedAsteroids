namespace Asteroids.Shared.GameObjects;

public record GameStateObject
{
    public GameState state { get; init; }
    public string LobbyName { get; init; }
    public List<Ship> ships { get; init; }
}



public enum GameState
{
    JOINING,
    PLAYING,

}