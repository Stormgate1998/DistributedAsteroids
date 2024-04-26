using Akka.Actor;
using Akka.DependencyInjection;
using Asteroids.Shared.GameObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Akka.Configuration;
using Akka.Cluster.Tools.Singleton;
using Akka.Dispatch.SysMsg;
using Akka.Event;
namespace Asteroids.Shared.Actors;


public class RemoteAkkaService : IHostedService
{
    private ActorSystem _actorSystem;
    private IActorRef lobbySupervisor;
    private IActorRef clientSupervisor;

    private IActorRef storageActor;
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;

    public RemoteAkkaService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string configString = Environment.GetEnvironmentVariable("ASTEROIDS_CLUSTER_CONFIG");
        var config = ConfigurationFactory.ParseString(configString);
        var bootstrap = BootstrapSetup.Create().WithConfig(config);

        var dependencyInjectionSetup = DependencyResolverSetup.Create(serviceProvider);
        var mergeSystemSetup = bootstrap.And(dependencyInjectionSetup);

        _actorSystem = ActorSystem.Create("je-actor-system", mergeSystemSetup);
        var cluster = Akka.Cluster.Cluster.Get(_actorSystem);

        if (cluster.SelfRoles.Contains("lobby"))
        {
            // var storageProps = ClusterSingletonProxy.Props(
            // singletonManagerPath: "/user/storageActor",
            // settings: ClusterSingletonProxySettings.Create(_actorSystem).WithRole("lobby")
            // );
            // storageActor = _actorSystem.ActorOf(storageProps, "storageActor");

            var storageActorProps = DependencyResolver.For(_actorSystem).Props<StorageActor>();
            var storageActor = _actorSystem.ActorOf(storageActorProps, "storageActor");


            var lobbyProps = DependencyResolver.For(_actorSystem).Props<LobbySupervisorActor>(storageActor);
            var singletonProps = ClusterSingletonManager.Props(
                singletonProps: lobbyProps,
                terminationMessage: PoisonPill.Instance,
                settings: ClusterSingletonManagerSettings.Create(_actorSystem).WithRole("lobby")
            );

            _actorSystem.ActorOf(singletonProps, "lobbyManagerSingleton");
        }


        var proxyProps = ClusterSingletonProxy.Props(
            singletonManagerPath: "/user/lobbyManagerSingleton",
            settings: ClusterSingletonProxySettings.Create(_actorSystem).WithRole("lobby")
        );

        lobbySupervisor = _actorSystem.ActorOf(proxyProps, "lobbySupervisorProxy");

        // lobbySupervisor = _actorSystem.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
        clientSupervisor = _actorSystem.ActorOf(Props.Create<ClientSupervisorActor>(lobbySupervisor, serviceProvider), "clientSupervisor");
        var deadletterWatchMonitorProps = Props.Create(() => new DeadletterMonitor());
        var deadletterWatchActorRef = _actorSystem.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");

        // subscribe to the event stream for messages of type "DeadLetter"
        _actorSystem.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _actorSystem.Terminate();
        return Task.CompletedTask;
    }

    public void CreateClient(string username, string hubConnection)
    {
        clientSupervisor.Tell(new CreateClientActor(username, hubConnection));
    }

    public void LeaveLobby(string username)
    {
        clientSupervisor.Tell(new LeaveLobby(username));
    }

    public async Task CreateLobby(string lobbyName)
    {
        Console.WriteLine("Requesting lobby from Akka service.");
        clientSupervisor.Tell(new CreateLobby(lobbyName));
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

    public async Task GetLobbies(string username)
    {
        clientSupervisor.Tell(new GetLobbies(username));
    }

    public async Task GetState(string lobby, string username)
    {
        clientSupervisor.Tell(new GetState(lobby, username));
    }

    public void SendShipInput(ShipInput input)
    {
        clientSupervisor.Tell(new SendShipInput(input));
    }

    public void KillLobby(string username)
    {
        clientSupervisor.Tell(new LobbyDeath(username));
    }

    public void UpdateExtras(string username, int extras)
    {
        Console.WriteLine($"Updating Extras: {extras}");
        clientSupervisor.Tell(new GameExtrasUpdate(extras, username));
    }
}
