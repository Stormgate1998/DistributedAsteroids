namespace ClientMessages;

public record EnterLobby(Ship UserShip);
public record CreateClientActor(string Username, string HubConnectionId);