namespace Asteroids.Shared.GameObjects;

public record Ship
{
    public string Username { get; init; }
    public int Health { get; init; }
    public int Speed { get; init; }
    public int Score { get; init; }
    public int Xpos { get; init; }
    public int Ypos { get; init; }
    public int Direction { get; init; }
    public bool MovingForward { get; init; }
    public bool TurningRight { get; init; }
    public bool TurningLeft { get; init; }



}
