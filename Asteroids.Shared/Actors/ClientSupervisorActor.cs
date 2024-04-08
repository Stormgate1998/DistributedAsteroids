using Akka.Actor;
using Asteroids.Shared.Services;

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
        HubService service = new(message.ConnectionId);
        IActorRef newClient = Context.ActorOf(Props.Create(() => new ClientActor(message.Username, LobbySupervisor, service)));
        clients.Add(message.Username, newClient);

        Sender.Tell(new CreateClientActorResponse(message.Username));

      }
      else
      {
        Sender.Tell(new CreateClientActorResponse($"Client {message.Username} already created"));

      }
    });

    Receive<CreateLobby>(message =>
    {
      if (clients.TryGetValue(message.LobbyName, out var user))
      {
        user.Forward(message);
      }
    });


    Receive<JoinLobby>(message =>
    {
      if (clients.TryGetValue(message.Username, out var user))
      {
        user.Forward(message);
      }
    });

    Receive<StartGame>(message =>
    {
      if (clients.TryGetValue(message.Username, out var user))
      {
        user.Forward(message);
      }
    });
    Receive<GetLobbies>(message =>
    {
      if (clients.TryGetValue(message.Username, out var user))
      {
        user.Forward(message);
      }

    });

    Receive<GetState>(message =>
    {
      if (clients.TryGetValue(message.Username, out var user))
      {
        user.Forward(message);
      }
    });

  }


  protected override void PreStart()
  {
    IActorRef lobbySupervisor = Context.ActorSelection("/user/lobbySupervisor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
    LobbySupervisor = lobbySupervisor;
  }

}
