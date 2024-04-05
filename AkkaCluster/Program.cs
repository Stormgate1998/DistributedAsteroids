using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// using Akka.DependencyInjection;

await Host.CreateDefaultBuilder(args)
.ConfigureServices((hostContext, services) =>
{
    services.AddHostedService<AkkaService>();
})
.RunConsoleAsync();
