using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<AkkaService>();
            })
            .RunConsoleAsync();
        Console.WriteLine("Hello, World!");
    }
}