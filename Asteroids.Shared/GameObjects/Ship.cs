namespace Asteroids.Shared.GameObjects;

public record Ship
{
    public string Username { get; init; }
    public int Health { get; init; }
    public int Speed { get; init; }
    public int Score { get; init; }
    public Location Location { get; init; }
    public int Direction { get; init; }
    public bool MovingForward { get; init; }
    public bool TurningRight { get; init; }
    public bool TurningLeft { get; init; }
    public bool IsFiring { get; init; }
    public bool IsTriple { get; init; }
    public bool HasExtraLife { get; init; }
    public bool HasDoubleDamage { get; init; }
    public bool CanFireBackwards { get; init; }
}

public record Location(int X, int Y);

public class ShipInput
{
    public string Username { get; set; }
    public bool Forward { get; set; }
    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool Firing { get; set; }
}
