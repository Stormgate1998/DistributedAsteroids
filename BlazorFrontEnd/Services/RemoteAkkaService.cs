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



/*
SuperVisor
Take in string (username) and use it to make LobbyActor with the name of that string

Have list of usernames/dictionary with connections 

ship:
location
direction
health
username


on keypress,
send moveleft,right, forward, backward, fire as




*/


*/
