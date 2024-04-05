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
                Sender.Tell(new CreateLobbyResponse($"Lobby '{message.LobbyName}' created."));
            }
            else
            {
                Sender.Tell(new CreateLobbyResponse($"Lobby '{message.LobbyName}' already exists."));
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
                var originalSender = Sender;
                lobby.Forward(obj);
            }
            else
            {
                Sender.Tell($"Lobby '{lobbyName}' not found.");
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



/*
SuperVisor
Take in string (username) and use it to make LobbyActor with the name of that string




*/
