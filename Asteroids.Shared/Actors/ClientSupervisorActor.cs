using Akka.Actor;

namespace Asteroids.Shared.Actors;

public class ClientSupervisorActor : ReceiveActor
{
  public ClientSupervisorActor()
  {
    Receive<CreateClientActor>(_ =>
    {
      Context.ActorOf<ClientActor>();
      Sender.Tell(new CreateClientActorResponse());
    });
  }
}
