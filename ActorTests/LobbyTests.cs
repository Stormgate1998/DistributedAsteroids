using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Akka.TestKit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Asteroids.Shared.Services;

namespace ActorTests;

public class LobbyTests : TestKit
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

    private void CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef supervisor)
    {
        var provider = getServiceProvider();

        var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor(provider)), "storageActor");
        probe = CreateTestProbe();

        var storageProbe = CreateTestProbe();
        supervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageActor)), "lobbySupervisor");
    }

    [Fact]
    public void LobbySupervisorCanCreateLobby()
    {
        CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef supervisor);

        supervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<CreateLobbyResponse>();

        response.Message.Should().Be("Lobby 'testLobby' created.");
    }
    [Fact]
    public void LobbySupervisorCanGetLobbyList()
    {
        CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef supervisor);

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
        CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef lobbySupervisor);

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
        CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();

        lobbySupervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        response.Lobbies.Should().BeEquivalentTo(["testLobby"]);
    }
    [Fact]
    public void LobbySupervisorCanKillExistingLobby()
    {
        string lobbyName = "testLobby";
        CreateProbeAndLobbySupervisor(out TestProbe probe, out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby(lobbyName), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        var lobby = Sys.ActorSelection($"/user/lobbySupervisor/{lobbyName}").ResolveOne(TimeSpan.FromSeconds(3)).Result;

        probe.Watch(lobby);

        lobbySupervisor.Tell(new LobbyDeath(lobbyName), probe.Ref);
        probe.ExpectTerminated(lobby);
    }

}
