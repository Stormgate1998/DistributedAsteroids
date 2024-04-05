using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using FluentAssertions.Equivalency;

namespace ActorTests;

public class ClientTests : TestKit
{
  [Fact]
  public void ClientSupervisorCanCreateClient()
  {
    var probe = CreateTestProbe();
    var clientSupervisor = Sys.ActorOf<ClientSupervisorActor>();

    clientSupervisor.Tell(new CreateClientActor("tony"), probe.Ref);
    var item = probe.ExpectMsg<CreateClientActorResponse>();
    item.Message.Should().Be("tony");
  }

  [Fact]
  public void ClientSupervisorCanNotCreateDuplicateClient()
  {
    var probe = CreateTestProbe();
    var clientSupervisor = Sys.ActorOf<ClientSupervisorActor>();

    clientSupervisor.Tell(new CreateClientActor("tony"), probe.Ref);
    probe.ExpectMsg<CreateClientActorResponse>();
    clientSupervisor.Tell(new CreateClientActor("tony"), probe.Ref);
    var item = probe.ExpectMsg<CreateClientActorResponse>();
    item.Message.Should().Be("Client tony already created");
  }

  [Fact]
  public void ClientCanCreateLobby()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
    var client = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

    client.Tell(new CreateLobby(""), probe.Ref);
    probe.ExpectMsg<CreateLobbyResponse>();

    lobbySupervisor.Tell(new GetLobbies(), probe.Ref);
    var response = probe.ExpectMsg<GetLobbiesResponse>();

    List<string> TestList = ["tony"];
    response.Lobbies.Should().BeEquivalentTo(TestList);
  }

  [Fact]
  public void ClientCanJoinExisitingLobby()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>("lobbySupervisor");
    var client = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

    lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
    probe.ExpectMsg<CreateLobbyResponse>();

    client.Tell(new JoinLobby("testLobby"), probe.Ref);
    probe.ExpectMsg<JoinLobbyResponse>();
  }


  [Fact]
  public void ClientCantJoinNonExisitingLobby()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>("lobbySupervisor");
    var client = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

    client.Tell(new JoinLobby("testLobby"), probe.Ref);
    var response = probe.ExpectMsg<JoinLobbyResponse>();
    response.Message.Should().Be("Could not find lobby testLobby.");
  }

  // [Fact]
  // public void ClientActorCanCreateShip()
  // {
  //   var probe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");

  //   var clientActor = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

  //   clientActor.Tell(new CreateLobby(""), probe.Ref);
  //   Thread.Sleep(100);
  //   clientActor.Tell(new JoinLobby());

  // }

  // [Fact]
  // public void ClientActorCanCreateShip()
  // {
  //   var probe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");

  //   var clientActor = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

  //   clientActor.Tell(new CreateLobby(""), probe.Ref);
  //   Thread.Sleep(100);
  //   clientActor.Tell(new JoinLobby());

  //  Ship updatedShip = new Ship(whatever);
  //  clientActor.Tell(new ShipUpdate(updatedShip))

  // }


}