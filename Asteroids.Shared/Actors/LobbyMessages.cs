using Asteroids.Shared.GameObjects;
using Akka.Actor;
namespace Asteroids.Shared.Actors;

public record CreateLobby(string LobbyName);
public record CreateLobbyResponse(string Message, IActorRef? Actor = null);

public record GetLobbies(string Username);
public record GetLobbiesResponse(List<string> Lobbies);

public record JoinLobby(string LobbyName, string Username);
public record JoinLobbyResponse(IActorRef Actor);

public record ProcessAllShipMovement();

public record ProcessOneTick();

public record LobbyError(string Message);
public record LobbyDeath(string LobbyName);
public record ShipUpdate(Ship Updated);


public record ShipsUpdate(List<Ship> Updated);


public record GetState(string LobbyName, string Username);
public record GameStateSnapshot(GameStateObject Game);

public record StartGame(string Username);



public record TestProcessMovement(Ship TestShip, string LobbyName);
public record TestProcessMovementList(List<Ship> TestShip, string LobbyName);
public record TestShipCollision(string LobbyName, Ship collidingShip, Asteroid asteroid);
public record TestBulletCollision(string LobbyName, Bullet collidingBullet, Asteroid asteroid);

public record TestingAddAsteroid(string LobbyName, Asteroid Asteroid);
public record TestingAddShip(string LobbyName, Ship Ship);
public record TestingAddBullet(string LobbyName, Bullet Bullet);

public record TestOneTick(string LobbyName, int SpawnInterval = 1);



public record BulletCollisionResult(bool result);
public record ShipCollisionResult(bool result);

public record SetLobbyGameState
{
  public string LobbyName { get; init; }
  public GameState State { get; init; }
  public List<IActorRef> SubscribedClients { get; init; }
  public List<Ship>? Ships { get; init; }
  public List<Asteroid>? Asteroids { get; init; }
  public List<Bullet>? Bullets { get; init; }
  public int? AsteroidSpawnInterval { get; init; }
};
public record AdvanceTicks();

public record CountDown(int Number);

public record StoreState(string Key, GameStateObject Value);
public record GetSavedState(string Key);
public record ReceiveSavedState(GameStateObject Stored);
public record RemoveSavedState(string Key);

public record LeaveLobby(string Username);
