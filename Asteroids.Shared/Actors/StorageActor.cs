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
            if (gameObjects.ContainsKey(message.Key))
            {
                Sender.Tell(new ReceiveSavedState(gameObjects[message.Key]));
            }
        });

    }
}
