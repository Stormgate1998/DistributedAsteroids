using Akka.Actor;
using Asteroids.Shared.Actors;
namespace Asteroids.Shared.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> lobbies = [];

    public LobbySupervisorActor()
    {
        Receive<CreateLobby>(message =>
        {
            if (!lobbies.ContainsKey(message.LobbyName))
            {
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(message.LobbyName, OnLobbyDeath)), message.LobbyName);

                lobbies.Add(message.LobbyName, newLobby);
                Sender.Tell(new CreateLobbyResponse($"Lobby '{message.LobbyName}' created.", Self));
            }
        });

        Receive<LobbyDeath>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }

        });

        Receive<GetLobbies>(message =>
        {
            List<string> availableLobbies = lobbies.Keys.ToList();
            Sender.Tell(new GetLobbiesResponse(availableLobbies));
        });

        Receive<(string, object)>(tuple =>
        {
            Console.WriteLine("Hit Receive string-Object on Lobby");
            Console.WriteLine($"{tuple.Item1}, {tuple.Item2}");
            var (lobbyName, obj) = tuple;
            if (lobbies.TryGetValue(lobbyName, out var lobby))
            {
                lobby.Forward(obj);
            }
            else
            {
                Sender.Tell($"Lobby '{lobbyName}' not found.");
            }
        });

        Receive<JoinLobby>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
            else
            {
                Sender.Tell(new JoinLobbyResponse($"Could not find lobby: {message.LobbyName}."));
            }
        });
    }

    private void OnLobbyDeath(string lobbyName)
    {
        if (lobbies.ContainsKey(lobbyName))
        {
            lobbies.Remove(lobbyName);
        }
    }
}
