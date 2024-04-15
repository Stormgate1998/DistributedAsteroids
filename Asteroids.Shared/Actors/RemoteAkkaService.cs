using Akka.Actor;
using Akka.DependencyInjection;
using Asteroids.Shared.GameObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Asteroids.Shared.Actors;


public class RemoteAkkaService : IHostedService
{
    private ActorSystem _actorSystem;
    private IActorRef lobbySupervisor;
    private IActorRef clientSupervisor;
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;

    public RemoteAkkaService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var dependencyInjectionSetup = DependencyResolverSetup.Create(serviceProvider);

        var bootstrap = BootstrapSetup.Create();
        var mergeSystemSetup = bootstrap.And(dependencyInjectionSetup);

        _actorSystem = ActorSystem.Create("je-actor-system", mergeSystemSetup);
        lobbySupervisor = _actorSystem.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
        clientSupervisor = _actorSystem.ActorOf(Props.Create<ClientSupervisorActor>(serviceProvider), "clientSupervisor");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _actorSystem.Terminate();
        return Task.CompletedTask;
    }



    // public async Task<string> CreateClient(string username, string hubConnection)
    // {
    //     var response = await clientSupervisor.Ask<CreateClientActorResponse>(new CreateClientActor(username, hubConnection));
    //     return response.Message;
    // }
    public void CreateClient(string username, string hubConnection)
    {
        clientSupervisor.Tell(new CreateClientActor(username, hubConnection));
    }

    public async Task<string> CreateLobby(string lobbyName)
    {
        Console.WriteLine("Requesting lobby from Akka service.");
        var response = await clientSupervisor.Ask<CreateLobbyResponse>(new CreateLobby(lobbyName));
        return response.Message;
    }
    public void JoinLobby(string username, string lobbyName)
    {
        clientSupervisor.Tell(new JoinLobby(lobbyName, username));
    }

    public void StartGame(string username)
    {
        Console.WriteLine("Starting game");
        clientSupervisor.Tell(new StartGame(username));
    }

    public async Task<List<string>> GetLobbies(string username)
    {
        var response = await clientSupervisor.Ask<GetLobbiesResponse>(new GetLobbies(username));
        return response.Lobbies;
    }

    public async Task<GameStateObject> GetState(string lobby, string username)
    {
        var response = await clientSupervisor.Ask<GameStateSnapshot>(new GetState(lobby, username));
        return response.Game;
    }

    public void SendShipInput(ShipInput input)
    {
        clientSupervisor.Tell(new SendShipInput(input));
    }
}
