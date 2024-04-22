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
        var storageProbe = CreateTestProbe();
        var supervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageProbe)));

        supervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<CreateLobbyResponse>();

        response.Message.Should().Be("Lobby 'testLobby' created.");
    }

    [Fact]
    public void LobbySupervisorCanGetLobbyList()
    {
        var probe = CreateTestProbe();

        var storageProbe = CreateTestProbe();
        var supervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageProbe)));

        supervisor.Tell(new CreateLobby("testLobby1"), probe.Ref);
        var response = probe.ExpectMsg<CreateLobbyResponse>();
        response.Message.Should().Be("Lobby 'testLobby1' created.");

        supervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var responselobbies = probe.ExpectMsg<GetLobbiesResponse>();
        responselobbies.Lobbies.Should().BeEquivalentTo(["testLobby1"]);


        supervisor.Tell(new CreateLobby("testLobby2"), probe.Ref);
        response = probe.ExpectMsg<CreateLobbyResponse>();
        response.Message.Should().Be("Lobby 'testLobby2' created.");

        supervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        responselobbies = probe.ExpectMsg<GetLobbiesResponse>();
        responselobbies.Lobbies.Should().BeEquivalentTo(["testLobby1", "testLobby2"]);


        supervisor.Tell(new CreateLobby("testLobby3"), probe.Ref);
        response = probe.ExpectMsg<CreateLobbyResponse>();
        response.Message.Should().Be("Lobby 'testLobby3' created.");


        supervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        responselobbies = probe.ExpectMsg<GetLobbiesResponse>();

        List<string> TestList = ["testLobby1", "testLobby2", "testLobby3"];
        responselobbies.Lobbies.Should().BeEquivalentTo(TestList);
    }

    [Fact]
    public void LobbySupervisorCanCreateMultipleLobbies()
    {
        var probe = CreateTestProbe();
        var storageProbe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageProbe)));

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
        var storageProbe = CreateTestProbe();
        var lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageProbe)));

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();

        lobbySupervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        response.Lobbies.Should().BeEquivalentTo(["testLobby"]);
    }
}
