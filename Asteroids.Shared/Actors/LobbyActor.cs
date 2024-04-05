using Akka.Actor;
using Asteroids.Shared.GameObjects;
namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;
    private List<Ship> ships = new();

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        this.onDeathCallback = onDeathCallback;

        Receive<LobbyDeath>(death =>
        {
            var self = Self;
            Context.Stop(self);

        });

        Receive<JoinLobby>(message =>
        {
            Sender.Tell(new JoinLobbyResponse(), Self);
        });

        // Receive<EnterLobby>(entry =>
        // {
        //     string userName = entry.UserShip.Username;
        //     bool noMatch = !ships.Any(ship => ship.Username == userName);
        //     if (noMatch)
        //     {
        //         Ship user = entry.UserShip;
        //         user.Direction = 45;
        //         user.Xpos = 50;
        //         user.Ypos = 50;
        //         user.Health = 50;
        //         user.Score = 0;
        //         ships.Add(user);
        //     }
        // });

    }

    protected override void PostStop()
    {
        base.PostStop();
        onDeathCallback?.Invoke(Self.Path.Name);
    }
}
