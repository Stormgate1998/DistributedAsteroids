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


}