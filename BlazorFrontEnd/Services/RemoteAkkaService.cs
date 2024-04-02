using Akka.Actor;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Akka.Remote;

using Akka.Actor;
using Akka.Routing;
using Akka.Configuration;

public class RemoteAkkaService : IHostedService, IActorBridge
{
    private ActorSystem _actorSystem;
    private IActorRef _router;

    public RemoteAkkaService()
    {

    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // var config = ConfigurationFactory.ParseString
        //     (@"
        //     akka {
        //         actor {
        //             provider = remote
        //         }
        //         remote {
        //             dot-netty.tcp {
        //                 hostname = ""localhost""
        //                 port = 8081
        //             }
        //         }
        //     }"
        //     );

        _actorSystem = ActorSystem.Create("BlazorActorSystem");
        _router = _actorSystem.ActorOf(Props.Create<MyActor>().WithRouter(new ConsistentHashingPool(5)), "myRouter");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _actorSystem.Terminate();
        return Task.CompletedTask;
    }

    public async Task<string> SendMessage(string key, string message)
    {
        // Send the message to the router with the given key
        var response = await _router.Ask<string>(new ConsistentHashableEnvelope(message, key));
        return response;
    }
}

public class MyActor : ReceiveActor
{
    private int i = 0;
    public MyActor()
    {
        Receive<string>(message =>
        {
            i += 1;
            Sender.Tell($"Received message: {message} {i} {Self.Path}");
        });
    }
}
