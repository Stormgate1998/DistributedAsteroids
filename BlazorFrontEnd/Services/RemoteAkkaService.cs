using Akka.Actor;
using Akka.Routing;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;

public class RemoteAkkaService : IHostedService
{
    private ActorSystem _actorSystem;
    private IActorRef lobbySupervisor;
    private IActorRef clientSupervisor;

    public RemoteAkkaService()
    {
        _actorSystem = ActorSystem.Create("BlazorActorSystem");
        lobbySupervisor = _actorSystem.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
        clientSupervisor = _actorSystem.ActorOf(Props.Create<ClientSupervisorActor>(), "clientSupervisor");
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



    public async Task<string> CreateClient(string username)
    {
        var response = await clientSupervisor.Ask<CreateClientActorResponse>(new CreateClientActor(username));
        return response.Message;
    }

    public async Task<string> CreateLobby(string lobbyName)
    {
        var response = await clientSupervisor.Ask<CreateLobbyResponse>(new CreateLobby(lobbyName));
        return response.Message;
    }
    public async Task<bool> JoinLobby(string username, string lobbyName)
    {
        var response = await clientSupervisor.Ask<JoinLobbyResponse>(new JoinLobby(username, lobbyName));
        return response != null;
    }

    public async Task<GameStateObject> StartGame(string username)
    {
        var response = await clientSupervisor.Ask<GameStateSnapshot>(new StartGame(username));
        return response.Game;
    }

    public async Task<List<string>> GetLobbies()
    {
        var response = await lobbySupervisor.Ask<GetLobbiesResponse>(new GetLobbies());
        return response.Lobbies;
    }
}
