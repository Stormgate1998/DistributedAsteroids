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

    clientSupervisor.Tell(new CreateClientActor(), probe.Ref);
    probe.ExpectMsg<CreateClientActorResponse>();
  }
}