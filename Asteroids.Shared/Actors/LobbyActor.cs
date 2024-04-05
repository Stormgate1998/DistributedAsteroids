using Akka.Actor;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private GameStateObject gameState;

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        this.onDeathCallback = onDeathCallback;

        Receive<LobbyDeath>(death =>
        {
            var self = Self;
            Context.Stop(self);

        });

        Receive<JoinLobby>(message =>
        {
            string userName = message.Username;
            bool noMatch = !gameState.ships.Any(ship => ship.Username == userName);
            if (noMatch)
            {
                Ship ship = new(message.Username)
                {
                    Direction = 45,
                    Xpos = 50,
                    Ypos = 50,
                    Health = 50,
                    Score = 0,

                };
                gameState.ships.Add(ship);
                Sender.Tell(new JoinLobbyResponse(), Self);
            }
            Sender.Tell(new JoinLobbyResponse("Username already registered for this lobby"));
        });

        Receive<GetState>(message =>
        {
            Sender.Tell(new CurrentGameState(gameState));
        });

    }
    protected override void PreStart()
    {
        gameState = new GameStateObject
        {
            state = GameState.JOINING
        };
    }

    protected override void PostStop()
    {
        base.PostStop();
        onDeathCallback?.Invoke(Self.Path.Name);
    }
}
