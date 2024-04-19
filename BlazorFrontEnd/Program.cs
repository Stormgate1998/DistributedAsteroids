using Asteroids.Shared.Services;
using BlazorFrontEnd.Services;
using Asteroids.Shared.Actors;

using OpenTelemetry.Resources;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

Uri collectorUri = new("http://je-asteroids-otel-collector:4317");

builder.Services.AddLogging(l =>
{
  l.AddOpenTelemetry(o =>
  {
    o.SetResourceBuilder(
      ResourceBuilder.CreateDefault().AddService("front_end"))
    .AddOtlpExporter(options =>
    {
      options.Endpoint = collectorUri;
    });
  });
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IHubService, HubService>();
builder.Services.AddSingleton<RemoteAkkaService>();
builder.Services.AddHostedService(sp =>
  sp.GetRequiredService<RemoteAkkaService>()
);
builder.Services.AddScoped<SignalRService>();

var app = builder.Build();

app.UseExceptionHandler("/Error");
app.UseHsts();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
