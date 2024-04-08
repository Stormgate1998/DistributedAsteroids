using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public string ConnectionId { get; private set; }
    public IActorRef? CurrentLobby { get; private set; }
    private readonly IActorRef LobbySupervisor;
    private IHubService _hubService;
    private readonly IServiceScope scope;

    public ClientActor(
        string username,
        string connectionId,
        IActorRef lobbySupervisor,
        IServiceProvider serviceProvider)
    {
        Username = username;
        ConnectionId = connectionId;
        CurrentLobby = null;
        LobbySupervisor = lobbySupervisor;
        scope = serviceProvider.CreateScope();
        _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

        Receive<CreateLobby>(createLobby =>
        {
            LobbySupervisor.Tell(createLobby);
        });

        Receive<CreateLobbyResponse>(response =>
        {
            IActorRef? result = response.Actor;
            if (result != null)
            {
                CurrentLobby = result;
            }
        });

        Receive<JoinLobby>(message =>
        {
            LobbySupervisor.Forward(message);
        });

        Receive<StartGame>(message =>
        {
            if (username == message.Username)
            {
                LobbySupervisor.Forward(message);

            }
        });

        Receive<GetState>(message =>
        {
            if (CurrentLobby != null)
            {
                CurrentLobby.Tell(message);
            }
        });

        Receive<GetLobbies>(message =>
        {
            LobbySupervisor.Tell(message);
        });

        ReceiveAsync<GetLobbiesResponse>(async message =>
        {
            Console.WriteLine("Sending list of lobbies to service.");
            await _hubService.SendLobbyList(message.Lobbies, ConnectionId);
        });

        // Make this function generic. Upon receiving new client state, forward to hub
        Receive<SendClientStateToHub>(message =>
        {
            _hubService.SendClientState(ConnectionId, message.state)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });

        Receive<ClientError>(error =>
        {
            Console.WriteLine("Got client related error.");
            Console.WriteLine(error.Message);
        });

        // Receive<UpdateShip>(updateShip =>
        // {
        //     // Logic to update ship
        //     // Assuming ship update details are in updateShip object
        // });
        // Receive<EnterLobby>(enterLobby =>
        // {

        // });
    }

    // protected override void PreStart()
    // {
    //     _hubService.StartAsync();
    // }
}

