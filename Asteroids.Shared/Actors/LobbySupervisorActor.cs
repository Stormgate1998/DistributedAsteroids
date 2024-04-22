using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> lobbies = [];

    private IActorRef StorageActor;

    public LobbySupervisorActor()
    {
        StorageActor = Context.ActorSelection("/user/storageActor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
        Receive<CreateLobby>(message =>
        {
            Console.WriteLine("Creating lobby in lobby supervisor.");

            if (!lobbies.ContainsKey(message.LobbyName))
            {
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(message.LobbyName, OnLobbyDeath, StorageActor, new Dictionary<string, IActorRef>())), message.LobbyName);

                lobbies.Add(message.LobbyName, newLobby);
                Context.Watch(newLobby);
                Sender.Tell(new CreateLobbyResponse($"Lobby '{message.LobbyName}' created.", Self));
            }
            else
            {
                Console.WriteLine("Lobby already exists. Skipping creation.");
            }
        });

        Receive<Terminated>(message =>
        {
            string key = lobbies.FirstOrDefault(x => x.Value == message.ActorRef).Key;
            lobbies.Remove(key);

            StorageActor.Tell(new GetSavedState(key));
        });

        Receive<ReceiveSavedState>(message =>
        {

            if (message.Stored.state != GameState.GAMEOVER)
            {
                var storedList = new Dictionary<string, IActorRef>();
                string LobbyName = message.Stored.LobbyName;
                foreach (var item in message.Stored.particpatingUsers)
                {
                    var actor = Context.ActorSelection(item.Value);
                    var actorRef = actor.ResolveOne(TimeSpan.FromSeconds(1)).Result;
                    storedList.Add(item.Key, actorRef);
                }
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(LobbyName, OnLobbyDeath, StorageActor, storedList)), LobbyName);

                lobbies.Add(LobbyName, newLobby);
                Context.Watch(newLobby);
                newLobby.Tell(new RehydrateState(message.Stored));
            }

        });

        Receive<LobbyDeath>(message =>
        {
            StorageActor.Tell(new GetSavedState(message.LobbyName));
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
                throw new Exception($"Could not find lobby: {message.LobbyName}.");
            }
        });

        Receive<GetState>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }

        });

        Receive<StartGame>(message =>
        {
            if (lobbies.TryGetValue(message.Username, out var lobby))
            {
                lobby.Forward(message);
            }

        });

        Receive<TestProcessMovement>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestProcessMovementList>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestShipCollision>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestBulletCollision>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });

        Receive<TestingAddAsteroid>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestingAddShip>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestingAddBullet>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
            }
        });
        Receive<TestOneTick>(message =>
        {
            if (lobbies.TryGetValue(message.LobbyName, out var lobby))
            {
                lobby.Forward(message);
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
