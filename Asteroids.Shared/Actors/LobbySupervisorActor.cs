using Akka.Actor;
using Akka.Event;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbySupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> lobbies = [];

    private List<string> LobbyNames = [];

    private IActorRef StorageActor;
    private readonly IActorRef Myself;

    protected override void PreStart()
    {
        
        var actorRef = Context.ActorSelection("/user/storageActor").ResolveOne(TimeSpan.FromSeconds(3)).Result;
        StorageActor = actorRef;

        Console.WriteLine($"Storage actor is {StorageActor.Path}. ");
        StorageActor.Tell(new TestMessage("Storage Actor is called"));

    }

    public LobbySupervisorActor(IActorRef storageActor)
    {
        Myself = Self;
        StorageActor = storageActor;

        if (LobbyNames.Count == 0)
        {
            StorageActor.Tell(new GetLobbyList());
        }

        Receive<RetrievedLobbyList>(message =>
        {
            if (message.List != null && message.List.Count > 0)
            {
                foreach (var item in message.List)
                {
                    StorageActor.Tell(new GetSavedState(item, Myself));
                }
            }

        });

        Receive<CreateLobby>(message =>
        {
            Console.WriteLine("Creating lobby in lobby supervisor.");

            if (!lobbies.ContainsKey(message.LobbyName))
            {
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(message.LobbyName, OnLobbyDeath, StorageActor, new Dictionary<string, IActorRef>())), message.LobbyName);

                lobbies.Add(message.LobbyName, newLobby);
                Console.WriteLine($"Created lobby {newLobby}");
                Console.WriteLine($"Lobby created is {lobbies[message.LobbyName]}");
                Sender.Tell(new CreateLobbyResponse($"Lobby '{message.LobbyName}' created.", Self));
                LobbyNames.Add(message.LobbyName);
                StorageActor.Tell(new StoreLobbyList(LobbyNames));
            }
            else
            {
                Console.WriteLine("Lobby already exists. Skipping creation.");
            }
        });

        Receive<ReceiveSavedState>(message =>
        {
            Console.WriteLine($"Bringing back {message.Stored.LobbyName}");

            if (message.Stored.state != GameState.GAMEOVER)
            {
                var storedList = new Dictionary<string, IActorRef>();
                string LobbyName = message.Stored.LobbyName;
                IActorRef newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(LobbyName, OnLobbyDeath, StorageActor, storedList)), LobbyName);
                foreach (var item in message.Stored.particpatingUsers)
                {
                    var actor = Context.ActorSelection(item.Value);
                    var actorRef = actor.ResolveOne(TimeSpan.FromSeconds(1)).Result;
                    storedList.Add(item.Key, actorRef);
                    actorRef.Tell(new UpdateLobby(newLobby));
                }

                lobbies.Add(LobbyName, newLobby);
                LobbyNames.Add(LobbyName);
                newLobby.Tell(new RehydrateState(message.Stored));
            }
            else
            {
                Console.WriteLine($"{message.Stored.LobbyName} already ended");
            }

        });

        Receive<LobbyDeath>(message =>
        {
            StorageActor.Tell(new GetSavedState(message.LobbyName, Self));
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
        Console.WriteLine($"Lobby {lobbyName} died");
        Console.WriteLine($"Lobby {lobbyName} died");
        lobbies.Remove(lobbyName);
        LobbyNames.Remove(lobbyName);

        Console.WriteLine("Getting back the saved state");
        StorageActor.Tell(new GetSavedState(lobbyName, Myself));
    }
}
