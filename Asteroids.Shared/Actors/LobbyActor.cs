using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using Akka.Actor;
using Akka.Util.Internal;
using Asteroids.Shared.GameObjects;

namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private string LobbyName;
    private Timer timer;
    private int Ticks = 0;
    private int AsteroidSpawnInterval = 30;
    private Dictionary<string, IActorRef> particpatingUsers = [];
    private GameStateObject gameState = new() { state = GameState.JOINING, ships = [], asteroids = [], bullets = [] };

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        IActorRef self = Self;
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
            GameStateObject newState = new()
            {
                state = GameState.PLAYING,
                ships = gameState.ships,
                asteroids = gameState.asteroids,
                bullets = gameState.bullets,
            };
            gameState = newState;
            StartTimer();
            Sender.Tell(new GameStateSnapshot(newState));

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
            Console.WriteLine($"Sending ship input to service. {DateTime.Now}");
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

                if (index == -1)
                {
                    Console.WriteLine($"Could not find index of ship to update. {JsonSerializer.Serialize(shipToUpdate)} | {JsonSerializer.Serialize(gameState.ships)}");
                }

                gameState.ships[index] = updatedShip;
            }
            else
            {
                Console.WriteLine("Could not find ship for client input");
                Console.WriteLine($"Username: {message.Input.Username} | Ships: {JsonSerializer.Serialize(gameState.ships)}");
            }
        });


        Receive<ProcessOneTick>(message =>
        {
            List<Bullet> updatedBullets = new(gameState.bullets);
            var updatedShips = ProcessAllShipMovement(gameState.ships);
            var newBullets = CreateAllBulletsThatShouldExist(updatedShips);
            if (newBullets.Count > 0)
            {
                updatedBullets.AddRange(newBullets);

            }

            List<Asteroid> updatedAsteroids = ProcessAsteroids(gameState.asteroids);

            Console.WriteLine($"Updated asteroids before processing collisions: {JsonSerializer.Serialize(updatedAsteroids)}");

            (updatedShips, updatedAsteroids, updatedBullets) =
            ProcessAllAsteroidCollisions(updatedShips, updatedAsteroids, updatedBullets);

            updatedShips = ProcessAllShipCollisions(updatedShips, updatedAsteroids);
            updatedBullets = RemoveOutOfBoundsBullets(updatedBullets);

            gameState = gameState with
            {
                state = gameState.state,
                ships = updatedShips,
                asteroids = updatedAsteroids,
                bullets = updatedBullets,
            };

            Console.WriteLine($"GAMESTATE: {JsonSerializer.Serialize(gameState)}");

            foreach (var user in particpatingUsers.Values)
            {
                user.Tell(new GameStateSnapshot(gameState));
            }

            Ticks++;
        });

        Receive<TestShipCollision>(message =>
        {
            Sender.Tell(new ShipCollisionResult(IsColliding(message.collidingShip, message.asteroid)));

        });

        Receive<TestBulletCollision>(message =>
        {
            Sender.Tell(new BulletCollisionResult(IsColliding(message.collidingBullet, message.asteroid)));
        });


        Receive<TestingAddAsteroid>(message =>
        {
            gameState.asteroids.Add(message.Asteroid);
        });
        Receive<TestingAddShip>(message =>
        {
            gameState.ships.Add(message.Ship);
            Sender.Tell(new GameStateSnapshot(gameState));
        });
        Receive<TestingAddBullet>(message =>
        {
            gameState.bullets.Add(message.Bullet);

        });
        Receive<TestOneTick>(message =>
        {
            AsteroidSpawnInterval = message.SpawnInterval;
            List<Bullet> updatedBullets = new(gameState.bullets);
            var updatedShips = ProcessAllShipMovement(gameState.ships);
            var newBullets = CreateAllBulletsThatShouldExist(updatedShips);
            if (newBullets.Count > 0)
            {
                updatedBullets.AddRange(newBullets);
            }

            List<Asteroid> updatedAsteroids = ProcessAsteroids(gameState.asteroids);
            (updatedShips, updatedAsteroids, updatedBullets) =
            ProcessAllAsteroidCollisions(updatedShips, updatedAsteroids, updatedBullets);

            updatedShips = ProcessAllShipCollisions(updatedShips, gameState.asteroids);
            updatedBullets = RemoveOutOfBoundsBullets(updatedBullets);

            gameState = gameState with
            {
                state = gameState.state,
                ships = updatedShips,
                asteroids = updatedAsteroids,
                bullets = updatedBullets,
            };
            Sender.Tell(new GameStateSnapshot(gameState));
        });

        Receive<SetLobbyGameState>(message =>
        {
            // Set gameState = message
        });

        Receive<AdvanceTicks>(message =>
        {
            // Advance timer by 1 tick
        });
    }

    private List<Asteroid> ProcessAsteroids(List<Asteroid> asteroids)
    {
        if (Ticks % AsteroidSpawnInterval == 0)
        {
            asteroids.Add(SpawnAsteroid());
        }

        return ProcessAllAsteroidMovement(asteroids);
    }

    private Asteroid SpawnAsteroid()
    {
        var rng = new Random();
        List<Location> spawnPoints = [new Location(1, 1), new Location(999, 1), new Location(1, 499), new Location(999, 499)];

        int selectedLocationIndex = rng.Next(0, 3);
        Location selectedLocation = spawnPoints[selectedLocationIndex];

        int direction = CalculateDirectionTowardsCenter(selectedLocation);
        int speed = DetermineAsteroidSpeed(rng);
        int size = DetermineAsteroidSize(rng);
        int health = CalculateAsteroidHealth(size, speed);

        return new Asteroid
        {
            Location = selectedLocation,
            Direction = direction,
            Health = health,
            Size = size,
            Speed = speed,
        };
    }

    private int CalculateDirectionTowardsCenter(Location spawnLocation)
    {
        double deltaX = 500 - spawnLocation.X;
        double deltaY = 250 - spawnLocation.Y;

        // Check if the spawn location is on the y-axis
        if (deltaX == 0)
        {
            // Handle the case when the spawn location is on the y-axis
            // The direction should be either directly above (180 degrees) or below (0 degrees) the center
            return deltaY >= 0 ? 0 : 180;
        }

        // Calculate the angle in radians from the spawn location to the center
        double angleRadians = Math.Atan2(deltaY, deltaX);

        // Convert radians to degrees
        double angleDegrees = angleRadians * (180 / Math.PI);

        // Adjust the angle to ensure it's between 0 and 360 degrees
        int direction = (int)((angleDegrees + 360) % 360);

        return direction;
    }

    private int DetermineAsteroidSpeed(Random rng)
    {
        return rng.Next(1, 10);
    }

    private int DetermineAsteroidSize(Random rng)
    {
        return rng.Next(1, 5);
    }

    private int CalculateAsteroidHealth(int size, int speed)
    {
        return size * speed * 1;
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

    private static List<Bullet> RemoveOutOfBoundsBullets(List<Bullet> bullets)
    {
        List<Bullet> returnedBullets = new(bullets);
        foreach (Bullet bullet in bullets)
        {
            bool xlimit = bullet.Location.X > 1000 || bullet.Location.X <= 0;
            bool ylimit = bullet.Location.Y > 500 || bullet.Location.Y <= 0;
            if (xlimit || ylimit)
            {
                returnedBullets.Remove(bullet);
            }
        }
        return returnedBullets;
    }

    private int TurnShipRight(Ship ship)
    {
        int newDirection = ship.Direction + 5;

        if (newDirection >= 360)
            newDirection -= 360;

        return newDirection;
    }

    private void StartTimer()
    {
        var self = Self;
        timer = new Timer(_ =>
                {
                    self.Tell(new ProcessOneTick());
                }, null, 0, 33);
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

        if (myShipList.Count != newShipList.Count())
        {
            Console.WriteLine($"Old: {JsonSerializer.Serialize(myShipList)} | New: {JsonSerializer.Serialize(newShipList)}");
            throw new Exception("Lost ship during processing.");
        }

        return newShipList;
    }

    public List<Asteroid> ProcessAllAsteroidMovement(List<Asteroid> asteroids)
    {
        Console.WriteLine("Processing movement of all asteroids.");

        List<Asteroid> copyOfAsteroids = new(asteroids);
        List<Asteroid> newAsteroids = [];

        foreach (Asteroid asteroid in copyOfAsteroids)
        {
            var updatedAsteroid = asteroid with
            {
                Location = CalculateNextPosition(asteroid.Location, asteroid.Speed, asteroid.Direction)
            };

            if (!HasHitBorder(asteroid))
            {
                Console.WriteLine("Has not the border.");
                newAsteroids.Add(updatedAsteroid);
            }
            else
            {
                Console.WriteLine("Has hit the border.");
            }
            
        }

        Console.WriteLine($"Copy of asteroids: {JsonSerializer.Serialize(newAsteroids)}");

        return newAsteroids;
    }

    private bool HasHitBorder(Asteroid asteroid)
    {
        if (asteroid.Location.X >= 1000
            || asteroid.Location.X <= 0
            || asteroid.Location.Y >= 500
            || asteroid.Location.Y <= 0)
        {
            return true;
        }

        return false;
    }

    public List<Ship> ProcessAllShipCollisions(List<Ship> shipList, List<Asteroid> AsteroidList)
    {
        List<Ship> newShipList = new(shipList);
        List<Asteroid> newAsteroidLIst = new(AsteroidList);

        List<Ship> returnedShipList = [];

        foreach (Ship ship in newShipList)
        {
            int hitcount = 0;
            foreach (Asteroid asteroid in newAsteroidLIst)
            {
                if (IsColliding(ship, asteroid))
                {
                    hitcount++;
                }

            }
            Ship newShip = ship with
            {
                Health = ship.Health - (5 * hitcount),
            };
            returnedShipList.Add(newShip);
        }
        return returnedShipList;
    }

    public (List<Ship>, List<Asteroid>, List<Bullet>) ProcessAllAsteroidCollisions
    (List<Ship> ships, List<Asteroid> asteroids, List<Bullet> bullets)
    {
        List<Ship> shipList = new(ships);
        List<Asteroid> asteroidsList = new(asteroids);
        List<Bullet> bulletList = new(bullets);

        Console.WriteLine($"Updated asteroids after processing collisions: {JsonSerializer.Serialize(asteroidsList)}");

        foreach (var bullet in bullets)
        {
            foreach (Asteroid asteroid in asteroids)
            {
                if (IsColliding(bullet, asteroid))
                {
                    int asteroidIndex = asteroids.FindIndex(x => x.Location == asteroid.Location);
                    var updatedAsteroid = asteroids[asteroidIndex] with { Health = asteroid.Health - 5 };
                    if (updatedAsteroid.Health <= 0)
                    {
                        asteroidsList.Remove(asteroid);
                        var shipIndex = shipList.FindIndex(ship => ship.Username == bullet.Username);
                        if (shipIndex != -1)
                        {
                            var updatedShip = shipList[shipIndex] with { Score = shipList[shipIndex].Score + asteroid.Speed * asteroid.Size };
                            shipList[shipIndex] = updatedShip;
                        }
                    }
                    else
                    {
                        asteroidsList[asteroidIndex] = updatedAsteroid;
                        var shipIndex = shipList.FindIndex(ship => ship.Username == bullet.Username);
                        if (shipIndex != -1)
                        {
                            var updatedShip = shipList[shipIndex] with { Score = shipList[shipIndex].Score + asteroid.Speed * asteroid.Size / 10 };
                            shipList[shipIndex] = updatedShip;
                        }
                    }

                    bulletList.Remove(bullet);
                    break;
                }

            }
        }

        Console.WriteLine($"Updated asteroids after processing collisions: {JsonSerializer.Serialize(asteroidsList)}");

        return (shipList, asteroidsList, bulletList);

    }


    public List<Bullet> CreateAllBulletsThatShouldExist(List<Ship> ships)
    {
        List<Bullet> newBulletList = [];


        foreach (Ship ship in ships)
        {
            if (ship.IsFiring)
            {
                Bullet bullet = new Bullet()
                {
                    Location = ship.Location,
                    Username = ship.Username,
                    Speed = 20,
                    Direction = ship.Direction,
                };
                newBulletList.Add(bullet);

            }
        }
        return newBulletList;
    }



    public bool IsColliding(Ship colliding, Asteroid asteroid)
    {
        return Distance(colliding.Location.X, asteroid.Location.X) + Distance(colliding.Location.Y, asteroid.Location.Y) <= (400 + (asteroid.Size * asteroid.Size));
    }

    public bool IsColliding(Bullet colliding, Asteroid asteroid)
    {
        return Distance(colliding.Location.X, asteroid.Location.X) + Distance(colliding.Location.Y, asteroid.Location.Y) <= (9 + (asteroid.Size * asteroid.Size));
    }

    public int Distance(int x, int y)
    {
        return (y - x) * (y - x);
    }

    protected override void PostStop()
    {
        base.PostStop();

        var self = Self;
        onDeathCallback?.Invoke(self.Path.Name);
    }
}
