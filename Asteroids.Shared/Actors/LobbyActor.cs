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
            Ship testShip = ProcessMovement(message.TestShip);
            Sender.Tell(new ShipUpdate(testShip));


        });

        Receive<TestProcessMovementList>(message =>
        {
            List<Ship> testShip = ProcessAllShipMovement(message.TestShip);
            Sender.Tell(new ShipsUpdate(testShip));
        });



    }
    public Ship ProcessMovement(Ship ship)
    {
        int direction = 0;

        if (ship.TurningRight == true)
        {
            direction = ship.Direction - 5;
        }
        else if (ship.TurningLeft == true)
        {
            direction = ship.Direction + 5;
        }
        else
        {
            direction = ship.Direction;
        }


        double angleInRadians = direction * Math.PI / 180.0;

        // Calculate the new x and y coordinates
        int newX = (int)(ship.Xpos + ship.Speed * Math.Cos(angleInRadians));
        int newY = (int)(ship.Ypos + ship.Speed * Math.Sin(angleInRadians));
        int speed = 0;
        if (ship.MovingForward)
        {
            speed = ship.Speed + 2;
            if (speed > 10)
            {
                speed = 10;
            }
        }
        else
        {
            speed = ship.Speed - 1;
            if (speed < 0)
            {
                speed = 0;
            }
        }
        return new Ship()
        {
            Username = ship.Username,
            Direction = direction,
            Xpos = newX,
            Ypos = newY,
            Health = ship.Health,
            Score = ship.Score,
            MovingForward = ship.MovingForward,
            TurningLeft = ship.TurningLeft,
            TurningRight = ship.TurningRight,
            Speed = speed,
        };
    }

    public List<Ship> ProcessAllShipMovement(List<Ship> shipList)
    {
        List<Ship> newShipList = [];
        foreach (Ship ship in shipList)
        {
            newShipList.Add(ProcessMovement(ship));
        }
        return newShipList;
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
