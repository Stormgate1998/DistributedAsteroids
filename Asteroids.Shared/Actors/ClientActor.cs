using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public string ConnectionId { get; private set; }
    public ClientState State { get; private set; }
    public IActorRef? CurrentLobby { get; private set; }
    private readonly IActorRef LobbySupervisor;
    private IHubService _hubService;
    private readonly ILogger<ClientActor> _logger;
    private readonly IServiceScope scope;

    public ClientActor(
        string username,
        string connectionId,
        IActorRef lobbySupervisor,
        ILogger<ClientActor> logger,
        IServiceProvider serviceProvider)
    {
        Username = username;
        ConnectionId = connectionId;
        CurrentLobby = null;
        LobbySupervisor = lobbySupervisor;
        _logger = logger;
        scope = serviceProvider.CreateScope();
        _hubService = scope.ServiceProvider.GetRequiredService<IHubService>();

        Receive<GetClientState>(message =>
        {
            _logger.LogInformation($"Getting state of client for {Username}.");
            Sender.Tell(new GetClientStateResponse(State));
        });

        Receive<CreateLobby>(createLobby =>
        {
            _logger.LogInformation($"{Username} is requesting lobby supervisor to create lobby.");
            LobbySupervisor.Tell(createLobby);
        });

        Receive<CreateLobbyResponse>(message =>
        {
            IActorRef? lobby = message.Actor;

            if (lobby != null)
            {
                _logger.LogInformation($"{Username} has created and joined lobby {lobby}.");
                CurrentLobby = lobby;
            }
        });

        Receive<JoinLobby>(message =>
        {
            _logger.LogInformation($"{Username} is requesting lobby supervisor to join {message.LobbyName}.");
            LobbySupervisor.Tell(message);
        });

        Receive<JoinLobbyResponse>(message =>
        {
            _logger.LogInformation($"{Username} has successfully joined lobby.");
            CurrentLobby = message.Actor;
            State = ClientState.InLobby;
        });

        Receive<StartGame>(message =>
        {
            if (username == message.Username)
            {
                _logger.LogInformation($"{Username} is requesting lobby {CurrentLobby} to start game.");
                CurrentLobby.Tell(message);
            }
        });

        Receive<LeaveLobby>(message =>
        {
            _logger.LogInformation($"{Username} is leaving lobby {CurrentLobby}.");
            CurrentLobby.Tell(message);
            CurrentLobby = null;
        });

        Receive<GetState>(message =>
        {
            if (CurrentLobby != null)
            {
                _logger.LogInformation($"{Username} is requesting state of lobby {CurrentLobby}.");
                CurrentLobby.Tell(message);
            }
        });

        Receive<GetLobbies>(message =>
        {
            _logger.LogInformation($"{Username} is requesting lobby supervisor to get the list of lobbies.");
            LobbySupervisor.Tell(message);
        });

        ReceiveAsync<GetLobbiesResponse>(async message =>
        {
            _logger.LogInformation(
                $"{Username} has received the list of lobbies from lobby supervisor.\n" +
                "Sending list to websocket hub service."
            );
            await _hubService.SendLobbyList(message.Lobbies, ConnectionId)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });


        Receive<CountDown>(message =>
        {
            _logger.LogInformation(
                $"{Username} has received countdown message.\n" +
                "Sending countdown to websocket hub service."
            );
            _hubService.SendCountDownNumber(message.Number, ConnectionId)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });

        // Make this function generic. Upon receiving new client state, forward to hub
        Receive<SendClientStateToHub>(message =>
        {
            _logger.LogInformation($"Sending {Username}'s state to websocket hub service.");
            _hubService.SendClientState(ConnectionId, message.state)
                .PipeTo(
                    Self,
                    failure: ex => new ClientError(ex.Message)
                );
        });

        Receive<ClientError>(error =>
        {
            _logger.LogInformation($"{Username} recieved a client-related error.");
            _logger.LogInformation(error.Message);
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
            _logger.LogInformation($"{Username} is requesting lobby {CurrentLobby} to die.");
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

        Receive<UpdateLobby>(message =>
        {
            CurrentLobby = message.Lobby;
        });
    }
}
