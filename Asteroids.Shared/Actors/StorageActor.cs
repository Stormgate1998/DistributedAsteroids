using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using Akka.Actor;
using Akka.Util.Internal;
using Asteroids.Shared.GameObjects;

namespace Asteroids.Shared.Actors;

public class StorageActor : ReceiveActor
{
    private Dictionary<string, GameStateObject> gameObjects = [];
    public StorageActor()
    {
        IActorRef self = Self;


        Receive<StoreState>(message =>
        {
            Console.WriteLine($"Asked to store game for {message.Key}");
            if (gameObjects.ContainsKey(message.Key))
            {
                gameObjects[message.Key] = message.Value;
            }
            else
            {
                gameObjects.Add(message.Key, message.Value);
            }
        });

        Receive<GetSavedState>(message =>
        {
            Console.WriteLine($"Asked to retrieve game for {message.Key}");
            if (gameObjects.TryGetValue(message.Key, out GameStateObject? value))
            {
                Sender.Tell(new ReceiveSavedState(value));
            }
        });

        Receive<RemoveSavedState>(message =>
        {
            Console.WriteLine($"Asked to remove game for {message.Key}");
            gameObjects.Remove(message.Key);
        });

        Receive<TestMessage>(message =>
        {
            Console.WriteLine($"{message.Content}");
        });

    }
}
