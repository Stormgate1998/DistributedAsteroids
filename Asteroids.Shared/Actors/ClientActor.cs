using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public string ConnectionId { get; private set; }
    public ClientState State { get; private set; }
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

        Receive<GetClientState>(message =>
        {
            Sender.Tell(new GetClientStateResponse(State));
        });

        Receive<CreateLobby>(createLobby =>
        {
            Console.WriteLine("Sending CreateLobby to lobby supervisor.");
            Console.WriteLine(LobbySupervisor);
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
            LobbySupervisor.Tell(message);
        });

        Receive<JoinLobbyResponse>(message =>
        {
            // var result = response.Actor;
            // if (result != null)
            // {
            // CurrentLobby = result;
            // }
            CurrentLobby = message.Actor;
            State = ClientState.InLobby;
        });

        Receive<StartGame>(message =>
        {
            if (username == message.Username)
            {
                CurrentLobby.Tell(message);

            }
        });

        Receive<LeaveLobby>(message =>
        {
            CurrentLobby.Tell(message);
            CurrentLobby = null;
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
            await _hubService.SendLobbyList(message.Lobbies, ConnectionId)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });


        Receive<CountDown>(message =>
        {
            Console.WriteLine("Received countdown message");
            _hubService.SendCountDownNumber(message.Number, ConnectionId)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
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

        Receive<GameStateSnapshot>(message =>
        {
            Console.WriteLine($"CLIENT ACTOR | Location: ({message.Game.ships.FirstOrDefault()?.Location.X}, {message.Game.ships.FirstOrDefault()?.Location.Y})");
            Console.WriteLine($"Actor ship count: {message.Game.ships.Count}");
            _hubService.SendGameSnapshot(ConnectionId, message.Game)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });

        Receive<SendShipInput>(message =>
        {
            CurrentLobby.Tell(message);
        });

        Receive<LobbyDeath>(message =>
        {
            CurrentLobby.Tell(message);
        });

        Receive<GameExtrasUpdate>(message =>
        {
            if (username == message.LobbyName)
            {
                Console.WriteLine($"Updating Game extras as Client:{message.Extras}");
                CurrentLobby.Tell(message);
            }

        });
    }
}

