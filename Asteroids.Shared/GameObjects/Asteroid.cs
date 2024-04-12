namespace Asteroids.Shared.GameObjects;

public record Asteroid
{
    public int Health { get; init; }
    public Location Location { get; init; }
    public int Direction { get; init; }
    public int Size { get; init; }
    public int Speed { get; init; }
}
