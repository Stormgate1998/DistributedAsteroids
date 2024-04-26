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
            Console.WriteLine($"Storing key {message.Key}");
            _service.StoreGameSnapshot(message.Key, message.Value);
        });

        Receive<GetSavedState>(message =>
        {
            Console.WriteLine($"Asked to retrieve game for {message.Key}");
            // if (gameObjects.TryGetValue(message.Key, out GameStateObject? value))
            // {
            //     Sender.Tell(new ReceiveSavedState(value));
            // }
            _service.GetGameSnapshot(message.Key)
            .PipeTo(
                message.Ref,
                success: (snapshot) =>
                {
                    snapshot = snapshot with
                    {
                        LobbyName = message.Key,
                    };
                    Console.WriteLine($"Game state as storageactor: {snapshot.state}");
                    Console.WriteLine($"Game ship count as storageactor: {snapshot.ships.Count}");
                    Console.WriteLine($"Game first player as storageactor: {snapshot.particpatingUsers.First().Value}");
                    return new ReceiveSavedState(snapshot);
                }
    );

        });

        Receive<RemoveSavedState>(message =>
        {
            Console.WriteLine($"Asked to remove game for {message.Key}");
            gameObjects.Remove(message.Key);
        });

        Receive<TestMessage>(message =>
        {
            Console.WriteLine(message.Content);
        });
        Receive<StoreLobbyList>(message =>
        {
            Console.WriteLine($"Storing lobby list");
            _service.StoreLobbyList(message.List);
        });

        Receive<GetLobbyList>(message =>
        {
            Console.WriteLine($"Storing lobby list");
            _service.GetLobbyList()
            .PipeTo(
                Sender,
                success: (list) =>
                {
                    return new RetrievedLobbyList(list);
                });
        });
    }
}
