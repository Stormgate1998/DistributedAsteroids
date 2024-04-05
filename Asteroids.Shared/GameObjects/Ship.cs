namespace Asteroids.Shared.GameObjects;

public class Ship
{
    public string Username;
    public int Health;
    public int Score;
    public int Xpos;
    public int Ypos;
    public int Direction;
    public bool MovingForward;
    public bool TurningRight;
    public bool TurningLeft;

    public Ship(string userName)
    {
        Username = userName;
        MovingForward = false;
        TurningLeft = false;
        TurningRight = false;
    }
}
