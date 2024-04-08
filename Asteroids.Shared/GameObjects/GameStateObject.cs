namespace Asteroids.Shared.GameObjects;

public class GameStateObject
{
    public GameState state;
    public List<Ship> ships;
    public GameStateObject()
    {
        ships = [];
        state = GameState.JOINING;
    }
}



public enum GameState
{
    JOINING,
    PLAYING,

}