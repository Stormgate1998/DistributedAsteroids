using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;

namespace ActorTests;

public class ClientTests : TestKit
{
  [Fact]
  public void CanCreateClientActor()
  {
    var probe = CreateTestProbe();
    var clientSupervisor = Sys.ActorOf<ClientSupervisorActor>();

    clientSupervisor.Tell(new CreateClientActor("tony"), probe.Ref);
    var item = probe.ExpectMsg<CreateClientActorResponse>();
    item.Message.Should().Be("tony");
  }

  [Fact]
  public void CantCreateDuplicateClientActor()
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
  public void ClientActorCanCreateLobby()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");

    var clientActor = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

    clientActor.Tell(new CreateLobby(""), probe.Ref);
    Thread.Sleep(100);

    lobbySupervisor.Tell(new GetLobbies(), probe.Ref);
    var response = probe.ExpectMsg<GetLobbiesResponse>();

    List<string> TestList = ["tony"];
    response.Lobbies.Should().BeEquivalentTo(TestList);
  }

  [Fact]
  public void ClientActorCanCreateShip()
  {
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");

    var clientActor = Sys.ActorOf(Props.Create<ClientActor>("tony"), "tony");

    clientActor.Tell(new CreateLobby(""), probe.Ref);
    Thread.Sleep(100);
    clientActor.Tell(new JoinLobby());

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

  //  Ship updatedShip = new Ship(whatever);
  //  clientActor.Tell(new ShipUpdate(updatedShip))

  // }


}