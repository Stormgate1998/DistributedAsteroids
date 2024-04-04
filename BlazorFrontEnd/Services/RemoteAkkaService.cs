using Akka.Actor;
using Akka.Routing;

public class RemoteAkkaService : IHostedService
{
    private ActorSystem _actorSystem;
    private IActorRef _router;

    public RemoteAkkaService()
    {
        _actorSystem = ActorSystem.Create("BlazorActorSystem");
        _router = _actorSystem.ActorOf<SupervisorActor>();
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // var config = ConfigurationFactory.ParseString
        //     (@"
        //     akka {
        //         actor {
        //             provider = remote
        //         }
        //         remote {
        //             dot-netty.tcp {
        //                 hostname = ""localhost""
        //                 port = 8081
        //             }
        //         }
        //     }"
        //     );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _actorSystem.Terminate();
        return Task.CompletedTask;
    }

    public async Task<string> CreateLobby(string lobbyName)
    {
        var response = await _router.Ask<string>((lobbyName, "CreateLobby"));
        return response;
    }

    public async Task<List<string>> GetLobbies()
    {
        var response = await _router.Ask<List<string>>("getLobbies");
        return response;
    }
    public async Task<string?> CommandToLobby(string lobbyName, object toLobby)
    {
        var response = await _router.Ask<string>((lobbyName, toLobby));
        return response;
    }
    // public async Task<string> SendMessage(string key, string message)
    // {
    //     // Send the message to the router with the given key
    //     var response = await _router.Ask<string>(new ConsistentHashableEnvelope(message, key));
    //     return response;
    // }
}

public class SupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> lobbies = new Dictionary<string, IActorRef>();

    public SupervisorActor()
    {
        Receive<(string, string)>(tuple =>
        {
            Console.WriteLine("Hit Receive string-string on Supervisor");
            var (lobbyName, command) = tuple;
            if (command == "CreateLobby")
            {
                if (!lobbies.ContainsKey(lobbyName))
                {
                    var newLobby = Context.ActorOf(Props.Create(() => new LobbyActor(lobbyName, OnLobbyDeath)), lobbyName);
                    lobbies.Add(lobbyName, newLobby);
                    Sender.Tell($"Lobby '{lobbyName}' created.");
                }
                else
                {
                    Sender.Tell($"Lobby '{lobbyName}' already exists.");
                }
            }
        });

        Receive<string>(message =>
            {
                if (message == "getLobbies")
                {
                    List<string> varlist = lobbies.Keys.ToList();
                    Sender.Tell(varlist);
                }
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

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        this.onDeathCallback = onDeathCallback;
        Receive<string>(message =>
        {
            var self = Self;
            if (message == "kill")
            {
                Context.Stop(self);
            }
            else if (message == "returnName")
            {
                Sender.Tell($"Lobby name: {lobbyName}, Path: {self.Path}");
            }
        });
        Receive<object>(obj =>
        {
            Sender.Tell("Not supported");
        });

    }

    protected override void PostStop()
    {
        base.PostStop();
        onDeathCallback?.Invoke(Self.Path.Name);
    }
}



/*
SuperVisor
Take in string (username) and use it to make LobbyActor with the name of that string




*/
