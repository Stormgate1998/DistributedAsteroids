using Akka.Actor;
using Asteroids.Shared.Actors;
using System.Collections.Generic;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public string HubConnectionId { get; private set; }
    public IActorRef? CurrentLobby { get; private set; }

    public ClientActor(string username)
    {
        Username = username;
        CurrentLobby = null;
        // HubConnectionId = hubConnectionId;

        Receive<CreateLobby>(createLobby =>
        {
            IActorRef lobbySupervisor = Context.ActorSelection("/user/lobbySupervisor").ResolveOne(TimeSpan.FromSeconds(3)).Result;

            lobbySupervisor.Tell(new CreateLobby(Username));

        });

        Receive<CreateLobbyResponse>(response =>
        {
            IActorRef result = response.Actor;
            if (result != null)
            {
                CurrentLobby = result;
            }
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

