using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;

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

        supervisor.Tell(new GetLobbies(), probe.Ref);
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

        lobbySupervisor.Tell(new GetLobbies(), probe.Ref);
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

        lobbySupervisor.Tell(new GetLobbies(), probe.Ref);
        var response = probe.ExpectMsg<GetLobbiesResponse>();

        response.Lobbies.Should().BeEquivalentTo(["testLobby"]);
    }

    // [Fact]
    // public void TestCanKillActor()
    // {
    //     var probe = CreateTestProbe();

    //     var supervisor = Sys.ActorOf<SupervisorActor>();
    //     supervisor.Tell(("testLobby1", "CreateLobby"), probe.Ref);
    //     probe.ExpectMsg<string>();
    //     supervisor.Tell(("testLobby2", "CreateLobby"), probe.Ref);
    //     probe.ExpectMsg<string>();
    //     supervisor.Tell(("testLobby3", "CreateLobby"), probe.Ref);
    //     probe.ExpectMsg<string>();

    //     supervisor.Tell("getLobbies", probe.Ref);
    //     var response = probe.ExpectMsg<List<string>>();
    //     List<string> TestList = ["testLobby1", "testLobby2", "testLobby3"];
    //     Assert.Equal(TestList, response);

    //     supervisor.Tell(("testeLobby1", "returnName"), probe.Ref);
    //     var responsestring = probe.ExpectMsg<string>();

    //     Assert.Equal($"Lobby name: testLobby1, Path: self.Path", responsestring);
    //     Thread.Sleep(100);
    //     supervisor.Tell(("testLobby2", "kill"));
    //     Thread.Sleep(100);
    //     supervisor.Tell("getLobbies", probe.Ref);
    //     response = probe.ExpectMsg<List<string>>();
    //     TestList = ["testLobby1", "testLobby3"];
    //     Assert.Equal(TestList, response);
    // }
}
