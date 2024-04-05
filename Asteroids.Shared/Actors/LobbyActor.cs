using Akka.Actor;

namespace Asteroids.Shared.Actors;

public class LobbyActor : ReceiveActor
{
    private readonly Action<string> onDeathCallback;

    public LobbyActor(string lobbyName, Action<string> onDeathCallback)
    {
        this.onDeathCallback = onDeathCallback;

        Receive<string>(message =>
        {
            var self = Self;
            if (message == "kill")
            {
                Context.Stop(self);
            }
            else if (message == "returnName")
            {
                Sender.Tell($"Lobby name: {lobbyName}, Path: {self.Path}");
            }
        });

        Receive<object>(obj =>
        {
            Sender.Tell("Not supported");
        });

    }

    protected override void PostStop()
    {
        base.PostStop();
        onDeathCallback?.Invoke(Self.Path.Name);
    }
}
