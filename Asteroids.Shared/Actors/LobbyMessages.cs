using Asteroids.Shared.GameObjects;
using Akka.Actor;
namespace Asteroids.Shared.Actors;

public record CreateLobby(string LobbyName);
public record CreateLobbyResponse(string Message, IActorRef? Actor = null);

public record GetLobbies(string Username);
public record GetLobbiesResponse(List<string> Lobbies);

public record JoinLobby(string LobbyName, string Username);
public record JoinLobbyResponse(IActorRef Actor);


public record LobbyError(string Message);
public record LobbyDeath(string LobbyName);
public record ShipUpdate(Ship Updated);


public record ShipsUpdate(List<Ship> Updated);


public record GetState(string LobbyName, string Username);
public record GameStateSnapshot(GameStateObject Game);

public record StartGame(string Username);



public record TestProcessMovement(Ship TestShip, string LobbyName);
public record TestProcessMovementList(List<Ship> TestShip, string LobbyName);
public record TestCollision(object )


