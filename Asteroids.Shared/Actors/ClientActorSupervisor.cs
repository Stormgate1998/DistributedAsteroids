using Akka.Actor;
using System.Collections.Generic;
namespace Asteroids.Shared.Actors;

public class ClientSupervisorActor : ReceiveActor
{
    private readonly Dictionary<string, IActorRef> _actors = new Dictionary<string, IActorRef>();

    public ClientSupervisorActor()
    {
        Receive<CreateClientActor>(createUserActor =>
        {
            var clientActor = Context.ActorOf(Props.Create(() => new ClientActor(createUserActor.Username, createUserActor.HubConnectionId)));
            _actors.Add(createUserActor.Username, clientActor);
        });

        Receive<CreateLobby>(createLobby =>
        {
            // Forward CreateLobby message to corresponding ClientActor
            if (_actors.TryGetValue(createLobby.LobbyName, out var clientActor))
            {
                clientActor.Forward(createLobby);
            }
        });

        Receive<ShipUpdate>(updateShip =>
        {
            // Forward UpdateShip message to corresponding ClientActor
            if (_actors.TryGetValue(updateShip.Username, out var clientActor))
            {
                clientActor.Forward(updateShip);
            }
        });

        Receive<EnterLobby>(entry =>
        {
            if (_actors.TryGetValue(entry.Username, out var clientActor))
            {
                clientActor.Forward(entry);
            }
        });
    }
}