using Akka.Actor;

namespace Asteroids.Shared.GameObjects;

public record GameStateObject
{
    public GameState state { get; init; }
    public string LobbyName { get; init; }
    public List<Ship> ships { get; init; }
    public List<Asteroid> asteroids { get; init; }
    public List<Bullet> bullets { get; init; }
    public Dictionary<string, IActorRef> particpatingUsers { get; init; }
}



public enum GameState
{
    JOINING,
    PLAYING,
    GAMEOVER,
}