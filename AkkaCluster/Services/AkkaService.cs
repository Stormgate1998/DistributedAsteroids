using Akka.Actor;
using Microsoft.Extensions.Hosting;
using Akka.Routing;
using Akka.Configuration;

public class AkkaService : IHostedService, IActorBridge
{
    private readonly ActorSystem _actorSystem;
    private readonly IActorRef _router;

    public AkkaService()
    {
        var config = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = cluster
                }
                remote {
                    dot-netty.tcp {
                        hostname = ""localhost""
                        port = 8081
                    }
                }
            }
            ");

        _actorSystem = ActorSystem.Create("MyActorSystem", config);

        // Create a ConsistentHashingPool router with 5 instances of the MyActor actor
        _router = _actorSystem.ActorOf(Props.Create<MyActor>().WithRouter(new ConsistentHashingPool(5)), "myRouter");
        Console.WriteLine("Ready");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // StartAsync method implementation
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _actorSystem.Terminate();
        Console.WriteLine("ending");
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
            Sender.Tell($"Received message: {message} {i} {Self.Path.ToString()}");
        });
    }
}
