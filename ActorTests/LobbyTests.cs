using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;

namespace ActorTests;

public class LobbyTests : TestKit
{
    [Fact]
    public void LobbySupervisorCanCreateLobby()
    {
        var probe = CreateTestProbe();
        var supervisor = Sys.ActorOf<LobbySupervisorActor>();

        supervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<CreateLobbyResponse>();

        response.Message.Should().Be("Lobby 'testLobby' created.");
    }

    [Fact]
    public void LobbySupervisorCanGetLobbyList()
    {
        var probe = CreateTestProbe();

        var supervisor = Sys.ActorOf<LobbySupervisorActor>();
        supervisor.Tell(new CreateLobby("testLobby1"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        supervisor.Tell(new CreateLobby("testLobby2"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        supervisor.Tell(new CreateLobby("testLobby3"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        supervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        List<string> TestList = ["testLobby1", "testLobby2", "testLobby3"];
        response.Lobbies.Should().BeEquivalentTo(TestList);
    }

    [Fact]
    public void LobbySupervisorCanCreateMultipleLobbies()
    {
        var probe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

        lobbySupervisor.Tell(new CreateLobby("testLobby1"), probe.Ref);
        var lobby1 = probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new CreateLobby("testLobby2"), probe.Ref);
        var lobby2 = probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        lobby1.Should().NotBe(lobby2);
        response.Lobbies.Should().BeEquivalentTo(["testLobby1", "testLobby2"]);
    }

    [Fact]
    public void LobbySupervisorCanNotCreateExisitingLobby()
    {
        var probe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();

        lobbySupervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        response.Lobbies.Should().BeEquivalentTo(["testLobby"]);
    }


    [Fact]
    public void MovementFunctionMovesShipHorizontal()
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
            Speed = 5,
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
        Ship testShip = new Ship()
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
            Speed = 5,
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
            Speed = 5,
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
        Ship testShip = new Ship()
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
            Speed = 5,
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
}
