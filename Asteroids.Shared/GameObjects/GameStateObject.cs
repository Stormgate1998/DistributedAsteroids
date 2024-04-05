namespace Asteroids.Shared.GameObjects;

public class GameStateObject
{
    public GameState state;
    public List<Ship> ships;
}



public enum GameState
{
    JOINING,
    PLAYING,

}