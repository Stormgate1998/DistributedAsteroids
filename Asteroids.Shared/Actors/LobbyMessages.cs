namespace Asteroids.Shared.Actors;

public record CreateLobby(string LobbyName);
public record CreateLobbyResponse(string Message);
public record GetLobbies();
public record GetLobbiesResponse(List<string> Lobbies);
public record LobbyError(string Message);