using System.Runtime.CompilerServices;
using Akka.Actor;
using Asteroids.Shared.Services;

namespace Asteroids.Shared.Actors;

public class ClientSupervisorActor : ReceiveActor
{
  private readonly Dictionary<string, IActorRef> clients = [];
  private IActorRef LobbySupervisor;

  public ClientSupervisorActor(IServiceProvider serviceProvider)
  {
    Receive<CreateClientActor>(message =>
    {
      if (!clients.ContainsKey(message.Username))
      {
        if (message.ConnectionId == null)
        {
          throw new NullReferenceException("Cannot create client actor. SignalR connection ID is null.");
        }

        IActorRef newClient = Context.ActorOf(Props.Create(() => new ClientActor(message.Username, message.ConnectionId, LobbySupervisor, serviceProvider)));
        clients.Add(message.Username, newClient);

        // Sender.Tell(new CreateClientActorResponse(message.Username));
        // newClient.Tell(new CreateClientActorResponse(message.Username)); // Send client state as well
        newClient.Tell(new SendClientStateToHub(message.Username, ClientState.NoLobby));
      }
      else
      {
        // Sender.Tell(new CreateClientActorResponse($"Client {message.Username} already created"));
      }
    });

    Receive<CreateLobby>(message =>
    {
      if (clients.TryGetValue(message.LobbyName, out var user))
      {
        user.Forward(message);
      }
      else
      {
        throw new Exception($"Cannot create lobby {message.LobbyName}. Client does not exist.");
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
