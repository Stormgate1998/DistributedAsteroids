using Asteroids.Shared.GameObjects;
using Akka.Actor;
namespace Asteroids.Shared.Actors;

public record CreateLobby(string LobbyName);
public record CreateLobbyResponse(string Message, IActorRef? Actor = null);

public record GetLobbies();
public record GetLobbiesResponse(List<string> Lobbies);

public record JoinLobby(string LobbyName);
public record JoinLobbyResponse();

public record LobbyError(string Message);
public record LobbyDeath(string LobbyName);
public record ShipUpdate(Ship Updated);
