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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 5,
            Ypos = 0,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 5,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 3,
            Ypos = 3,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 0,
            Direction = 5,
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
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 0,
            Direction = -5,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 5,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 4,
            Ypos = 0,
            Direction = 5,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();

        var testShipList = new List<Ship>
        {
            new() {
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Xpos = 0,
                Ypos = 0,
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
                Xpos = 0,
                Ypos = 0,
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
                Xpos = 0,
                Ypos = 0,
                Direction = 5,
                MovingForward = false,
                TurningLeft = true,
                TurningRight = false,
            },
            new(){
                Username = "tony",
                Health = 40,
                Score = 40,
                Speed = 0,
                Xpos = 0,
                Ypos = 0,
                Direction = -5,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 2,
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

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();
        Ship testShip = new Ship()
        {
            Username = "tony",
            Health = 40,
            Score = 40,
            Speed = 0,
            Xpos = 0,
            Ypos = 0,
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
            Xpos = 0,
            Ypos = 0,
            Direction = 90,
            MovingForward = true,
            TurningLeft = false,
            TurningRight = false,

        };
        lobbySupervisor.Tell(new TestProcessMovement(testShip2, "testLobby"), probe.Ref);
        response = probe.ExpectMsg<ShipUpdate>();
        response.Updated.Speed.Should().Be(10);

    }

}
