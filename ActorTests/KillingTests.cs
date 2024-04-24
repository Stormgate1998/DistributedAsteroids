using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
using Akka.Dispatch.SysMsg;
using Akka.TestKit;

namespace ActorTests;

public class KillingTests : TestKit
{
    private void CreateProbeAndSupervisor(out TestProbe probe, out IActorRef supervisor)
    {
        var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor()), "storageActor");
        probe = CreateTestProbe();
        var storageProbe = CreateTestProbe();
        supervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor()));
    }



    [Fact]
    public void LobbySupervisorCanCreateLobby()
    {
        CreateProbeAndSupervisor(out TestProbe probe, out IActorRef supervisor);

        supervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        var response = probe.ExpectMsg<CreateLobbyResponse>();

        response.Message.Should().Be("Lobby 'testLobby' created.");
    }


    [Fact]
    public void LobbySupervisorCanGetLobbyList()
    {
        CreateProbeAndSupervisor(out TestProbe probe, out IActorRef supervisor);


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
        CreateProbeAndSupervisor(out TestProbe probe, out IActorRef lobbySupervisor);

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
        CreateProbeAndSupervisor(out TestProbe probe, out IActorRef lobbySupervisor);

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectMsg<CreateLobbyResponse>();

        lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
        probe.ExpectNoMsg();

        lobbySupervisor.Tell(new GetLobbies("TestUser"), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        response.Lobbies.Should().BeEquivalentTo(["testLobby"]);
    }


}
