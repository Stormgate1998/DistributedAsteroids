using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
using Akka.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Asteroids.Shared.Services;

namespace ActorTests;

public class MovementTests : TestKit
{
    private ServiceProvider getServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        var raftServiceMock = Substitute.For<IRaftService>();
        services.AddSingleton(_ => raftServiceMock);

        var provider = services.BuildServiceProvider();
        return provider;
    }

    private void CreateLobbySupervisor(out IActorRef lobbySupervisor)
    {
        var provider = getServiceProvider();

        var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor(provider)), "storageActor");
        var storageProbe = CreateTestProbe();
        lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageActor)), "lobbySupervisor");
    }

    [Fact]
    public void MovementFunctionMovesShipHorizontal()
    {
        var probe = CreateTestProbe();

        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Location = new(0, 0),
            Direction = 0,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 4,
            Location = new(5, 0),
            Direction = 0,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
    }



    [Fact]
    public void MovementFunctionMovesShipVertical()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,
        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 4,
            Location = new(0, 5),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
    }

    [Fact]
    public void MovementFunctionMovesShipTilted()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Location = new(0, 0),
            Direction = 45,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,
        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 4,
            Location = new(3, 3),
            Direction = 45,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,
        };
        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
    }

    [Fact]
    public void MovementFunctionRotatesShipLeftAndRight()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 0,
            MovingForward = false,
            TurningLeft = true,
            TurningRight = false,
        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 355,
            MovingForward = false,
            TurningLeft = true,
            TurningRight = false,
        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
        Ship testShip2 = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 0,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = true,
        };

        Ship expectedShip2 = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 5,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = true,
        };
        lobbySupervisor.Tell(new TestProcessMovement(testShip2, "testLobby"), probe.Ref);
        response = probe.ExpectMsg<ShipUpdate>();
        response.Updated.Should().Be(expectedShip2);
    }

    [Fact]
    public void TestProcessMovementDoesMovementAndTurningRight()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Location = new(0, 0),
            Direction = 0,
            MovingForward = false,
            TurningLeft = true,
            TurningRight = false,
        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 4,
            Location = new(4, 0),
            Direction = 355,
            MovingForward = false,
            TurningLeft = true,
            TurningRight = false,
        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
    }

    [Fact]
    public void TestProcessesListOfShipsCorrectly()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        var testShipList = new List<Ship>
        {
            new() {
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Location = new(0, 0),
                Direction = 0,
                MovingForward = false,
                TurningLeft = true,
                TurningRight = false
            },
            new() {
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Location = new(0, 0),
                Direction = 0,
                MovingForward = false,
                TurningLeft = false,
                TurningRight = true
            }
        };

        var expectedShipList = new List<Ship>{
            new(){
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Location = new(0, 0),
                Direction = 355,
                MovingForward = false,
                TurningLeft = true,
                TurningRight = false,
            },
            new(){
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Location = new(0, 0),
                Direction = 5,
                MovingForward = false,
                TurningLeft = false,
                TurningRight = true,
            }

        };

        lobbySupervisor.Tell(new TestProcessMovementList(testShipList, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipsUpdate>();

        response.Updated.Should().BeEquivalentTo(expectedShipList);
    }

    [Fact]
    public void TestMoveForwardIncreasesSpeedBy2AndNotDecreasesBy1()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = true,
            TurningLeft = false,
            TurningRight = false,

        };

        Ship expectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 2,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = true,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
        Ship testShip2 = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 2,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };
        Ship secondExpectedShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 1,
            Location = new(0, 2),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,
        };
        lobbySupervisor.Tell(new TestProcessMovement(testShip2, "testLobby"), probe.Ref);
        response = probe.ExpectMsg<ShipUpdate>();
        response.Updated.Should().Be(secondExpectedShip);

    }

    [Fact]
    public void TestSpeedUpperAndLowerBoundsSet()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Speed.Should().Be(0);

        Ship testShip2 = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 10,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = true,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip2, "testLobby"), probe.Ref);
        response = probe.ExpectMsg<ShipUpdate>();
        response.Updated.Speed.Should().Be(10);
    }

    [Fact]
    public void TestCollisionShipRegistersTrue()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        Asteroid asteroid = new Asteroid()
        {
            Health = 20,
            Location = new(0, 20),
            Direction = 0,
            Size = 20,
            Speed = 0
        };
        lobbySupervisor.Tell(new TestShipCollision("testLobby", testShip, asteroid), probe.Ref);
        var response = probe.ExpectMsg<ShipCollisionResult>();
        response.result.Should().Be(true);
    }


    [Fact]
    public void TestCollisionShipRegistersFalse()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, -40),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        Asteroid asteroid = new Asteroid()
        {
            Health = 20,
            Location = new(0, 20),
            Direction = 0,
            Size = 20,
            Speed = 0
        };
        lobbySupervisor.Tell(new TestShipCollision("testLobby", testShip, asteroid), probe.Ref);
        var response = probe.ExpectMsg<ShipCollisionResult>();

        response.result.Should().Be(false);
    }

    [Fact]
    public void TestCollisionBulletRegistersTrue()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Bullet testBullet = new Bullet()
        {
            Username = "tony",
            Speed = 0,
            Location = new(0, 0),
            Direction = 90,
        };

        Asteroid asteroid = new Asteroid()
        {
            Health = 20,
            Location = new(0, 20),
            Direction = 0,
            Size = 20,
            Speed = 0
        };
        lobbySupervisor.Tell(new TestBulletCollision("testLobby", testBullet, asteroid), probe.Ref);
        var response = probe.ExpectMsg<BulletCollisionResult>();
        response.result.Should().Be(true);
    }

    [Fact]
    public void TestCollisionBulletRegistersFalse()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Bullet testBullet = new Bullet()
        {
            Username = "tony",
            Speed = 0,
            Location = new(0, 50),
            Direction = 90,
        };

        Asteroid asteroid = new Asteroid()
        {
            Health = 20,
            Location = new(0, 20),
            Direction = 0,
            Size = 20,
            Speed = 0
        };
        lobbySupervisor.Tell(new TestBulletCollision("testLobby", testBullet, asteroid), probe.Ref);
        var response = probe.ExpectMsg<BulletCollisionResult>();

        response.result.Should().Be(false);
    }

    [Fact]
    public async void TestShipCollisionRemovesHealth()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(0, 0),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        Asteroid asteroid = new()
        {
            Health = 20,
            Location = new(0, 20),
            Direction = 0,
            Size = 20,
            Speed = 0
        };

        lobbySupervisor.Tell(new TestingAddShip("testLobby", testShip));
        lobbySupervisor.Tell(new TestingAddAsteroid("testLobby", asteroid));

        await Task.Delay(100);

        lobbySupervisor.Tell(new TestOneTick("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<GameStateSnapshot>();
        response.Game.asteroids.Count.Should().Be(1);
        Ship ship = response.Game.ships[0];
        ship.Health.Should().Be(35);

    }

    [Fact]
    public async void TestBulletCollisionRemovesHealthAndIncreasesScore()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 0,
            Speed = 0,
            Location = new(-90, -90),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,

        };

        Asteroid asteroid = new()
        {
            Health = 20,
            Location = new(0, 90),
            Direction = 0,
            Size = 20,
            Speed = 1
        };

        Bullet bullet = new()
        {
            Username = "tony",
            Speed = 0,
            Location = new(0, 87),
            Direction = 0,
        };

        lobbySupervisor.Tell(new TestingAddShip("testLobby", testShip));
        lobbySupervisor.Tell(new TestingAddAsteroid("testLobby", asteroid));
        lobbySupervisor.Tell(new TestingAddBullet("testLobby", bullet));
        await Task.Delay(100);

        lobbySupervisor.Tell(new TestOneTick("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<GameStateSnapshot>();
        response.Game.asteroids.Count.Should().Be(1);
        response.Game.bullets.Count.Should().Be(0);
        Ship ship = response.Game.ships[0];
        ship.Health.Should().Be(35);
        // ship.Score.Should().Be(2);
        response.Game.asteroids.Count.Should().Be(1);

        // response.Game.asteroids[0].Health.Should().Be(15);



    }

    [Fact]
    public void TestIsFiringCreatesABullet()
    {
        var probe = CreateTestProbe();
        CreateLobbySupervisor(out IActorRef lobbySupervisor);

        List<string> strings = [];

        List<string> newStrigns = ["hi", "hi"];
        strings.AddRange(newStrigns);
        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();
        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Location = new(45, 45),
            Direction = 90,
            MovingForward = false,
            TurningLeft = false,
            TurningRight = false,
            IsFiring = true,
        };

        Bullet bullet = new()
        {
            Location = new(45, 45),
            Direction = 90,
            Speed = 20,
            Username = "tony",
        };

        lobbySupervisor.Tell(new TestingAddShip("testLobby", testShip), probe.Ref);
        var response = probe.ExpectMsg<GameStateSnapshot>();
        response.Game.ships.Count.Should().Be(1);



        lobbySupervisor.Tell(new TestOneTick("testLobby"), probe.Ref);
        response = probe.ExpectMsg<GameStateSnapshot>();
        response.Game.ships.Count.Should().Be(1);
        response.Game.bullets.Count.Should().Be(1);
        response.Game.bullets[0].Should().Be(bullet);

    }

    [Fact]
    public void LobbyCanSpawnAsteroid()
    {
        CreateLobby(out TestProbe probe, out IActorRef lobby);

        lobby.Tell(new TestOneTick("testLobby", 1), probe.Ref);
        var snapshot = probe.ExpectMsg<StoreState>();

        snapshot.Value.asteroids.Count.Should().Be(1);
    }

    private void CreateLobby(out TestProbe probe, out IActorRef lobby)
    {
        probe = CreateTestProbe();
        var newOne = probe;
        lobby = Sys.ActorOf(Props.Create(() => new LobbyActor("testLobby", newOne, new Dictionary<string, IActorRef>())), "testLobby");
    }

    [Fact]
    public void AstroidMovesAfterOneTick()
    {
        CreateLobby(out TestProbe probe, out IActorRef lobby);

        lobby.Tell(new TestOneTick("testLobby", 100), probe.Ref);
        var firstSnapshot = probe.ExpectMsg<StoreState>();

        lobby.Tell(new TestOneTick("testLobby", 100), probe.Ref);
        _ = probe.ExpectMsg<GameStateSnapshot>();
        lobby.Tell(new TestOneTick("testLobby", 100), probe.Ref);
        _ = probe.ExpectMsg<StoreState>();
        lobby.Tell(new TestOneTick("testLobby", 100), probe.Ref);
        GameStateSnapshot secondSnapshot = probe.ExpectMsg<GameStateSnapshot>();

        firstSnapshot.Value.asteroids.First().Location.X.Should().NotBe(secondSnapshot.Game.asteroids.First().Location.X);
        firstSnapshot.Value.asteroids.First().Location.Y.Should().NotBe(secondSnapshot.Game.asteroids.First().Location.Y);
    }
}
