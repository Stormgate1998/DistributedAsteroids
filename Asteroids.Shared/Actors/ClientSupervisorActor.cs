using Akka.Actor;

namespace Asteroids.Shared.Actors;

public class ClientSupervisorActor : ReceiveActor
{
  private readonly Dictionary<string, IActorRef> clients = [];
  private IActorRef LobbySupervisor;

  public ClientSupervisorActor()
  {

    Receive<CreateClientActor>(message =>
    {
      if (!clients.ContainsKey(message.Username))
      {
        IActorRef newClient = Context.ActorOf(Props.Create(() => new ClientActor(message.Username, LobbySupervisor)));
        clients.Add(message.Username, newClient);

        Sender.Tell(new CreateClientActorResponse(message.Username));

      }
      else
      {
        Sender.Tell(new CreateClientActorResponse($"Client {message.Username} already created"));

      }
    });
  }

  
    protected override void PreStart()
    {
      IActorRef lobbySupervisor = Context.ActorSelection("/user/lobbySupervisor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
      LobbySupervisor = lobbySupervisor;
    }

}
