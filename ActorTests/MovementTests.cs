using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;

namespace ActorTests;

public class MovementTests : TestKit
{
    [Fact]
    public void MovementFunctionMovesShipHorizontal()
    {
        var probe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
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
        response.Game.asteroids[0].Health.Should().Be(15);
        response.Game.bullets.Count.Should().Be(0);
        Ship ship = response.Game.ships[0];
        ship.Health.Should().Be(40);
        ship.Score.Should().Be(2);

    }

    [Fact]
    public async Task TestIsFiringCreatesABullet()
    {
        var probe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

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
            IsFiring = true
        };
        lobbySupervisor.Tell(new TestingAddShip("testLobby", testShip));

        await Task.Delay(100);

        lobbySupervisor.Tell(new TestOneTick("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<GameStateSnapshot>();
        response.Game.bullets.Count.Should().Be(1);
        response.Game.bullets[0].Location.Should().Be(new Location(0, 10));

    }
}
