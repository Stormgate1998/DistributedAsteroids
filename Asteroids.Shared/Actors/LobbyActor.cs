using Akka.Actor;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private string LobbyName;
    private GameStateObject gameState;

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        LobbyName = lobbyName;
        this.onDeathCallback = onDeathCallback;

        Receive<LobbyDeath>(death =>
        {
            var self = Self;
            Context.Stop(self);

        });

        Receive<JoinLobby>(message =>
        {
            string userName = message.Username;

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
        });

        Receive<GetState>(message =>
        {
            Sender.Tell(new GameStateSnapshot(gameState));
        });

        Receive<StartGame>(message =>
        {
            if (message.Username == LobbyName)
            {
                gameState.state = GameState.PLAYING;
                Sender.Tell(new GameStateSnapshot(gameState));
            }
        });

    }
    protected override void PreStart()
    {
        gameState = new GameStateObject
        {
            state = GameState.JOINING,
            ships = []
        };
    }

    protected override void PostStop()
    {
        base.PostStop();
        onDeathCallback?.Invoke(Self.Path.Name);
    }
}
