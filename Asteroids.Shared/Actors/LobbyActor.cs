using System.Threading;
using Akka.Actor;
using Asteroids.Shared.GameObjects;

namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private string LobbyName;
    private Timer timer;
    private Dictionary<string, IActorRef> particpatingUsers = [];
    private GameStateObject gameState = new() { state = GameState.JOINING, ships = [] };

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        LobbyName = lobbyName;
        this.onDeathCallback = onDeathCallback;

        timer = new Timer(_ =>
        {
            var updatedShips = ProcessAllShipMovement(gameState.ships);

            var newState = new GameStateObject
            {
                state = gameState.state,
                ships = updatedShips,
            };

            gameState = newState;
            //TODO
            //If all ships have health less than 0
            //self.tell gameover
            //in gameover, timer.Dispose();

            foreach (var user in particpatingUsers.Values)
            {
                user.Tell(new GameStateSnapshot(gameState));
            }

        }, null, 0, 10);

        Receive<LobbyDeath>(death =>
        {
            var self = Self;
            Context.Stop(self);

        });

        Receive<JoinLobby>(message =>
        {
            string userName = message.Username;
            particpatingUsers[message.Username] = Sender;

            Ship ship = new()
            {
                Username = userName,
                Direction = 45,
                Location = new(100, 100),
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

        Receive<SendShipInput>(message =>
        {
            var shipToUpdate = gameState.ships.FirstOrDefault(ship => ship.Username == message.Input.Username);

            if (shipToUpdate != null)
            {
                Ship updatedShip = shipToUpdate with
                {
                    MovingForward = message.Input.Forward,
                    TurningLeft = message.Input.Left,
                    TurningRight = message.Input.Right,
                };

                // Update the ship in the collection
                int index = gameState.ships.IndexOf(shipToUpdate);
                gameState.ships[index] = updatedShip;
            }
        });
    }

    public Ship ProcessMovement(Ship ship)
    {
        int direction = ship.TurningLeft
            ? TurnShipLeft(ship)
            : ship.TurningRight
            ? TurnShipRight(ship)
            : ship.Direction;

        Location location = CalculateNextPosition(ship.Location, ship.Speed, direction);

        int speed = ship.MovingForward
            ? ship.Speed + 2
            : ship.Speed - 1;

        speed = Math.Clamp(speed, 0, 10);

        return ship with
        {
            Direction = direction,
            Location = location,
            Speed = speed,
        };
    }

    private Location CalculateNextPosition(Location location, int speed, int direction)
    {
        double angleInRadians = direction * Math.PI / 180.0;

        int xPosition = (int)(location.X + speed * Math.Cos(angleInRadians));
        int yPosition = (int)(location.Y + speed * Math.Sin(angleInRadians));

        return new Location
        (
            X: Math.Clamp(xPosition, 0, 1000),
            Y: Math.Clamp(yPosition, 0, 500)
        );
    }

    private int TurnShipRight(Ship ship)
    {
        int newDirection = ship.Direction + 5;

        if (newDirection >= 360)
            newDirection -= 360;

        return newDirection;
    }

    private int TurnShipLeft(Ship ship)
    {
        int newDirection = ship.Direction - 5;

        if (newDirection <= 0)
            newDirection += 360;

        return newDirection;
    }

    public List<Ship> ProcessAllShipMovement(List<Ship> shipList)
    {
        List<Ship> myShipList = new(shipList);
        List<Ship> newShipList = [];

        foreach (Ship ship in myShipList)
        {
            newShipList.Add(ProcessMovement(ship));
        }

        return newShipList;
    }

    public bool IsColliding(Ship colliding, Asteroid asteroid)
    {
        return (Distance(colliding.Location.X, colliding.Location.Y) + Distance(asteroid.Location.X, asteroid.Location.Y) <= (400 + (asteroid.Size * asteroid.Size)));
    }

    public bool IsColliding(Bullet colliding, Asteroid asteroid)
    {
        return (Distance(colliding.Location.X, colliding.Location.Y) + Distance(asteroid.Location.X, asteroid.Location.Y) <= (9 + (asteroid.Size * asteroid.Size)));
    }

    public int Distance(int x, int y)
    {
        return (x * x + y * y);
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
