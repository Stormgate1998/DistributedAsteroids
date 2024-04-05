using Akka.Actor;
using Asteroids.Shared.Actors;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    // public string HubConnectionId { get; private set; }
    public IActorRef? CurrentLobby { get; private set; }
    private readonly IActorRef LobbySupervisor;

    public ClientActor(string username, IActorRef lobbySupervisor)
    {
        Username = username;
        CurrentLobby = null;
        LobbySupervisor = lobbySupervisor;
        // HubConnectionId = hubConnectionId;

        Receive<CreateLobby>(createLobby =>
        {
            IActorRef lobbySupervisor = Context.ActorSelection("/user/lobbySupervisor").ResolveOne(TimeSpan.FromSeconds(3)).Result;

            lobbySupervisor.Forward(new CreateLobby(Username));
        });

        Receive<CreateLobbyResponse>(response =>
        {
            IActorRef result = response.Actor;
            if (result != null)
            {
                CurrentLobby = result;
            }
        });

        Receive<JoinLobby>(message =>
        {
            LobbySupervisor.Forward(message);
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
}

