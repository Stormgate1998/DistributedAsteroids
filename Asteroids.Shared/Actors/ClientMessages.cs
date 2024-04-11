using Asteroids.Shared.GameObjects;
using Asteroids.Shared.Services;

namespace Asteroids.Shared.Actors;

// public record EnterLobby(Ship UserShip);
public record CreateClientActor(string Username, string ConnectionId);
// public record CreateClientActorResponse(string Message);
public record SendClientStateToHub(string Message, ClientState state);

public record GetClientState();
public record GetClientStateResponse(ClientState State);

public record ClientError(string Message);

public record SendShipInput(ShipInput Input);