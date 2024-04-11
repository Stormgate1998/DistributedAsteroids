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
            // if (message.Username == gameState.LobbyName)
            // {
            GameStateObject newState = new()
            {
                state = GameState.PLAYING,
                ships = gameState.ships,
            };
            gameState = newState;
            Sender.Tell(new GameStateSnapshot(newState));
            // }
        });

        Receive<TestProcessMovement>(message =>
        {
            Ship testShip = ProcessMovement(message.ship);
            Sender.Tell(new ShipUpdate(testShip));


        });



    }
    public Ship ProcessMovement(Ship ship)
    {
        int direction = 0;

        if (ship.TurningRight == true)
        {
            direction = ship.Direction - 5;
        }

        double angleInRadians = ship.Direction * Math.PI / 180.0;

        // Calculate the new x and y coordinates
        int newX = (int)(ship.Xpos + ship.Speed * Math.Cos(angleInRadians));
        int newY = (int)(ship.Ypos + ship.Speed * Math.Sin(angleInRadians));

        return new Ship()
        {
            Username = ship.Username,
            Direction = ship.Direction,
            Xpos = newX,
            Ypos = newY,
            Health = ship.Health,
            Score = ship.Score,
            MovingForward = ship.MovingForward,
            TurningLeft = ship.TurningLeft,
            TurningRight = ship.TurningRight,
            Speed = ship.Speed,
        };
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
