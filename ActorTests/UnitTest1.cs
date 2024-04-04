using Akka.Actor;
using Akka.TestKit;
using FluentAssertions;
using Akka.TestKit.Xunit2;
namespace ActorTests;

public class UnitTest1 : TestKit
{
    [Fact]
    public void TestCanCreateLobby()
    {
        var probe = CreateTestProbe();

        var supervisor = Sys.ActorOf<SupervisorActor>();
        supervisor.Tell(("testLobby", "CreateLobby"), probe.Ref);
        var response = probe.ExpectMsg<string>();

        Assert.Equal("Lobby 'testLobby' created.", response);
    }

    [Fact]
    public void TestCanGetLobbyList()
    {
        var probe = CreateTestProbe();

        var supervisor = Sys.ActorOf<SupervisorActor>();
        supervisor.Tell(("testLobby1", "CreateLobby"));
        supervisor.Tell(("testLobby2", "CreateLobby"));
        supervisor.Tell(("testLobby3", "CreateLobby"));
        supervisor.Tell("getLobbies", probe.Ref);
        var response = probe.ExpectMsg<List<string>>();
        List<string> TestList = ["testLobby1", "testLobby2", "testLobby3"];
        Assert.Equal(TestList, response);
    }

    [Fact]
    public void TestCanKillActor()
    {
        var probe = CreateTestProbe();

        var supervisor = Sys.ActorOf<SupervisorActor>();
        supervisor.Tell(("testLobby1", "CreateLobby"));
        supervisor.Tell(("testLobby2", "CreateLobby"));
        supervisor.Tell(("testLobby3", "CreateLobby"));

        supervisor.Tell("getLobbies", probe.Ref);
        var response = probe.ExpectMsg<List<string>>();
        List<string> TestList = ["testLobby1", "testLobby2", "testLobby3"];
        Assert.Equal(TestList, response);

        supervisor.Tell(("testeLobby1", "returnName"), probe.Ref);
        var responsestring = probe.ExpectMsg<string>();

        Assert.Equal($"Lobby name: testLobby1, Path: self.Path", responsestring);
        Thread.Sleep(100);
        supervisor.Tell(("testLobby2", "kill"));
        Thread.Sleep(100);
        supervisor.Tell("getLobbies", probe.Ref);
        response = probe.ExpectMsg<List<string>>();
        TestList = ["testLobby1", "testLobby3"];
        Assert.Equal(TestList, response);
    }
}
