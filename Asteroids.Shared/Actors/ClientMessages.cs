namespace Asteroids.Shared.Actors;

// public record EnterLobby(Ship UserShip);
public record CreateClientActor(string Username, string ConnectionId);
public record CreateClientActorResponse(string Message);
