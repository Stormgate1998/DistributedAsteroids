using Akka.Actor;
using Asteroids.Shared.Actors;
using Asteroids.Shared.Services;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public IActorRef? CurrentLobby { get; private set; }
    private readonly IActorRef LobbySupervisor;
    private IHubService _hubService;

    public ClientActor(string username, IActorRef lobbySupervisor, IHubService hubService)
    {
        Username = username;
        CurrentLobby = null;
        LobbySupervisor = lobbySupervisor;
        _hubService = hubService;

        Receive<CreateLobby>(createLobby =>
        {
            LobbySupervisor.Forward(new CreateLobby(Username));
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

        Receive<GetLobbiesResponse>(async message =>
        {
            Console.WriteLine("Sending list of lobbies to service.");
            await _hubService.SendLobbyList(message.Lobbies);
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

