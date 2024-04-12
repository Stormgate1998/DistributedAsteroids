namespace Asteroids.Shared.GameObjects;

public record Bullet
{
    public string Username { get; init; }
    public int Speed { get; init; }
    public Location Location { get; init; }
    public int Direction { get; init; }
}