using Asteroids.Shared.Services;
using BlazorFrontEnd.Services;
using Asteroids.Shared.Actors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IHubService, HubService>();
builder.Services.AddSingleton<RemoteAkkaService>();
builder.Services.AddHostedService(sp =>
  sp.GetRequiredService<RemoteAkkaService>()
);
builder.Services.AddSingleton<SignalRService>();
// builder.Services.AddSingleton<HubConnection>(sp =>
// {
//     var hubConnection = new HubConnectionBuilder()
//         .WithUrl("http://je-asteroids-signalr/asteroidsHub")
//         .Build();
//     hubConnection.StartAsync();
//     return hubConnection;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
// }

// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
