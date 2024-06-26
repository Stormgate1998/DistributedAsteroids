using System.Runtime.CompilerServices;
using Akka.Actor;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asteroids.Shared.Actors;

public class ClientSupervisorActor : ReceiveActor
{
  private readonly Dictionary<string, IActorRef> clients = [];
  private IActorRef LobbySupervisor;

  public ClientSupervisorActor(IActorRef lobbySupervisor, IServiceProvider serviceProvider)
  {
    LobbySupervisor = lobbySupervisor;

    Receive<CreateClientActor>(message =>
    {
      if (!clients.ContainsKey(message.Username))
      {
        if (message.ConnectionId == null)
        {
          throw new NullReferenceException("Cannot create client actor. SignalR connection ID is null.");
        }

        var logger = serviceProvider.GetRequiredService<ILogger<ClientActor>>();

        IActorRef newClient = Context.ActorOf(Props.Create(() => new ClientActor(message.Username, message.ConnectionId, LobbySupervisor, logger, serviceProvider)));
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
    Receive<LeaveLobby>(message =>
    {
      if (clients.TryGetValue(message.Username, out var user))
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
      else
      {
        throw new Exception($"Client Supervisor: Could not find client {message.Username}.");
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

    Receive<SendShipInput>(message =>
    {
      if (clients.TryGetValue(message.Input.Username, out var user))
      {
        user.Forward(message);
      }
    });

    Receive<LobbyDeath>(message =>
    {
      if (clients.TryGetValue(message.LobbyName, out var user))
      {
        user.Forward(message);
      }
    });

    Receive<GameExtrasUpdate>(message =>
    {
      if (clients.TryGetValue(message.LobbyName, out var user))
      {
        Console.WriteLine($"Updating Game extras:{message.Extras}");
        user.Forward(message);
      }

    });
  }


  // protected override void PreStart()
  // {
  //   IActorRef lobbySupervisor = Context.ActorSelection("/user/lobbiesSingletonManager").ResolveOne(TimeSpan.FromSeconds(3)).Result;
  //   LobbySupervisor = lobbySupervisor;
  // }

}
