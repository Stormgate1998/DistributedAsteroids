using System.Data;
using Akka.Actor;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private string LobbyName;
    private GameStateObject gameState = new() { state = GameState.JOINING, ships = [] };

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        LobbyName = lobbyName;
        this.onDeathCallback = onDeathCallback;
        gameState = new()
        {
            state = GameState.JOINING
        };

        Receive<LobbyDeath>(death =>
        {
            var self = Self;
            Context.Stop(self);

        });

        Receive<JoinLobby>(message =>
        {
            string userName = message.Username;

            Ship ship = new()
            {
                Username = userName,
                Direction = 45,
                Xpos = 50,
                Ypos = 50,
                Health = 50,
                Score = 0,
            };

            gameState.ships.Add(ship);

            var self = Self;
            Sender.Tell(new JoinLobbyResponse(self));
        });

        Receive<GetState>(message =>
        {
            Sender.Tell(new GameStateSnapshot(gameState));
        });

        Receive<StartGame>(message =>
        {
            if (message.Username == LobbyName)
            {
                GameStateObject newState = new GameStateObject()
                {
                    state = GameState.PLAYING,
                    ships = gameState.ships,
                };
                gameState = newState;
                Sender.Tell(new GameStateSnapshot(newState));
            }
        });

    }
    
    // protected override void PreStart()
    // {
    //     gameState = new GameStateObject
    //     {
    //         state = GameState.JOINING,
    //         ships = []
    //     };
    // }

    protected override void PostStop()
    {
        base.PostStop();
        
        var self = Self;
        onDeathCallback?.Invoke(self.Path.Name);
    }
}
