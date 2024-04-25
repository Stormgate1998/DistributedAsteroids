using Asteroids.Shared.Actors;
using Asteroids.Shared.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

Uri collectorUri = new("http://je-asteroids-otel-collector:4317");

builder.Services.AddLogging(l =>
{
  l.AddOpenTelemetry(o =>
  {
    o.SetResourceBuilder(
      ResourceBuilder.CreateDefault().AddService("akka_cluster"))
    .AddOtlpExporter(options =>
    {
      options.Endpoint = collectorUri;
    });
  });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var baseAddress = "http://je-raft-gateway:8080";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddSingleton<IRaftService, RaftService>();

builder.Services.AddSingleton<IHubService, HubService>();
builder.Services.AddSingleton<RemoteAkkaService>();
builder.Services.AddHostedService(sp =>
  sp.GetRequiredService<RemoteAkkaService>()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.Run();
