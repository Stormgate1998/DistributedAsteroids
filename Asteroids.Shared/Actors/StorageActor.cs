using Akka.Actor;
using Asteroids.Shared.GameObjects;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Asteroids.Shared.Actors;

public class StorageActor : ReceiveActor
{
    private Dictionary<string, GameStateObject> gameObjects = [];
    private IRaftService _service;

    public StorageActor(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        _service = scope.ServiceProvider.GetRequiredService<IRaftService>();

        IActorRef self = Self;

        Receive<StoreState>(message =>
        {
            // Console.WriteLine($"Asked to store game for {message.Key}");
            // if (gameObjects.ContainsKey(message.Key))
            // {
            //     gameObjects[message.Key] = message.Value;
            // }
            // else
            // {
            //     gameObjects.Add(message.Key, message.Value);
            // }

            _service.StoreGameSnapshot(message.Key, message.Value)
                .PipeTo(Self);
        });

        Receive<GetSavedState>(message =>
        {
            // Console.WriteLine($"Asked to retrieve game for {message.Key}");
            // if (gameObjects.TryGetValue(message.Key, out GameStateObject? value))
            // {
            //     Sender.Tell(new ReceiveSavedState(value));
            // }

            _service.GetGameSnapshot(message.Key)
                .PipeTo(
                    Sender,
                    success: (snapshot) => new ReceiveSavedState(snapshot)
                );
        });

        Receive<RemoveSavedState>(message =>
        {
            Console.WriteLine($"Asked to remove game for {message.Key}");
            gameObjects.Remove(message.Key);
        });
    }
}
