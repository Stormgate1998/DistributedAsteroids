using Akka.Actor;
using Asteroids.Shared.Actors;
using System.Collections.Generic;

public class ClientActor : ReceiveActor
{
    public string Username { get; private set; }
    public string HubConnectionId { get; private set; }
    public string CurrentLobby { get; private set; }

    public ClientActor(string username)
    {
        Username = username;
        // HubConnectionId = hubConnectionId;

        Receive<CreateLobby>(createLobby =>
        {
            // Logic to create lobby
            CurrentLobby = createLobby.LobbyName;
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

