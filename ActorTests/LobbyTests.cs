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
            MovingForward = true,
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
            MovingForward = true,
            TurningLeft = false,
            TurningRight = false,

        };

        lobbySupervisor.Tell(new TestProcessMovement(testShip, "testLobby"), probe.Ref);
        var response = probe.ExpectMsg<ShipUpdate>();

        response.Updated.Should().Be(expectedShip);
    }
}
