using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> lobbies = [];

    private IActorRef StorageActor;

    protected override void PreStart()
    {
        var actorRef = Context.ActorSelection("/user/storageActor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
        if (actorRef != null)
        {
            StorageActor = actorRef;
        }
        Console.WriteLine($"Storage actor is {StorageActor.Path}. ");
        StorageActor.Tell(new TestMessage("Storage Actor is called"));

    }

    public LobbySupervisorActor(IActorRef storageActor)
    {
        StorageActor = storageActor;

        Receive<CreateLobby>(message =>
        {
            Console.WriteLine("Creating lobby in lobby supervisor.");

            if (!lobbies.ContainsKey(message.LobbyName))
            {
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(message.LobbyName, StorageActor, new Dictionary<string, IActorRef>())), message.LobbyName);

                lobbies.Add(message.LobbyName, newLobby);
                Console.WriteLine($"Created lobby {newLobby}");
                Console.WriteLine($"Lobby created is {lobbies[message.LobbyName]}");
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
            Console.WriteLine($"Lobby {message} died");
            string key = lobbies.FirstOrDefault(x => x.Value == message.ActorRef).Key;
            Console.WriteLine($"Lobby {key} died");
            lobbies.Remove(key);

            StorageActor.Tell(new GetSavedState(key));
        });

        Receive<ReceiveSavedState>(message =>
        {
            Console.WriteLine($"Bringing back {message.Stored.LobbyName}");

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
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(LobbyName, StorageActor, storedList)), LobbyName);

                lobbies.Add(LobbyName, newLobby);
                Context.Watch(newLobby);
                newLobby.Tell(new RehydrateState(message.Stored));
            }
            else
            {
                Console.WriteLine($"{message.Stored.LobbyName} already ended");
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

        Receive<GameExtrasUpdate>(message =>
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
        // if (lobbies.ContainsKey(lobbyName))
        // {
        //     lobbies.Remove(lobbyName);
        // }
    }
}
