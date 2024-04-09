using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Microsoft.Extensions.Logging;

namespace ActorTests;

public class ClientTests : TestKit
{
  private (ServiceProvider provider, IHubService realTimeMock) getServiceProvider()
  {
    var services = new ServiceCollection();
    services.AddLogging(builder => builder.AddConsole());
    var realTimeMock = Substitute.For<IHubService>();
    services.AddSingleton(_ => realTimeMock);

    var provider = services.BuildServiceProvider();
    return (provider, realTimeMock);
  }

  [Fact]
  public async void ClientSupervisorCanCreateClient()
  {
    var probe = CreateTestProbe();
    var (provider, signalRMock) = getServiceProvider();

    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");

    var clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(provider)), "clientSupervisor");
    clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);

    await Task.Delay(100);
    await signalRMock.Received().SendClientState(Arg.Any<string>(), Arg.Is<ClientState>(s => s == ClientState.NoLobby));
  }

  [Fact]
  public async void ClientSupervisorCanNotCreateDuplicateClient()
  {
    var probe = CreateTestProbe();
    var (provider, signalRMock) = getServiceProvider();

    _ = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
    var clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(provider)), "clientSupervisor");

    clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);
    // probe.ExpectMsg<CreateClientActorResponse>();

    await Task.Delay(100);
    await signalRMock.Received(1).SendClientState(Arg.Any<string>(), Arg.Is<ClientState>(s => s == ClientState.NoLobby));

    signalRMock.ClearReceivedCalls();

    clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);

    await Task.Delay(100);
    await signalRMock.DidNotReceive().SendClientState(Arg.Any<string>(), Arg.Is<ClientState>(s => s == ClientState.NoLobby));
  }
  //5, 6 me
  [Fact]
  public async void ClientCanCreateLobby()
  {
    var (provider, signalRMock) = getServiceProvider();

    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
    var client = Sys.ActorOf(Props.Create<ClientActor>("tony", "fake id", lobbySupervisor, provider), "tony");

    client.Tell(new CreateLobby("tony"));
    await Task.Delay(100);


    lobbySupervisor.Tell(new GetLobbies("intentionally left blank"), probe.Ref);
    var response = probe.ExpectMsg<GetLobbiesResponse>();

    List<string> TestList = ["tony"];
    response.Lobbies.Should().BeEquivalentTo(TestList);
  }

  // [Fact]
  // public void ClientCanJoinExisitingLobby()
  // {
  //   var serviceMock = new Mock<IHubService>();

  //   var probe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>("lobbySupervisor");
  //   var client = Sys.ActorOf(Props.Create<ClientActor>("tony", lobbySupervisor, serviceMock.Object), "tony");

  //   lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
  //   probe.ExpectMsg<CreateLobbyResponse>();

  //   client.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
  //   probe.ExpectMsg<JoinLobbyResponse>();
  // }


  // [Fact]
  // public void ClientCannotJoinNonExisitingLobby()
  // {
  //   var serviceMock = new Mock<IHubService>();

  //   var probe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf<LobbySupervisorActor>("lobbySupervisor");
  //   var client = Sys.ActorOf(Props.Create<ClientActor>("tony", lobbySupervisor, serviceMock.Object), "tony");

  //   client.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
  //   var response = probe.ExpectMsg<JoinLobbyResponse>();
  //   response.Message.Should().Be("Could not find lobby: testLobby.");
  // }


  // [Fact]
  // public void ClientSupervisorCanJoinLobby()
  // {
  //   var probe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
  //   var clientSupervisor = Sys.ActorOf<ClientSupervisorActor>();

  //   clientSupervisor.Tell(new CreateClientActor("tony", ""), probe.Ref);
  //   probe.ExpectMsg<CreateClientActorResponse>();

  //   lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
  //   probe.ExpectMsg<CreateLobbyResponse>();

  //   clientSupervisor.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
  //   probe.ExpectMsg<JoinLobbyResponse>();
  // }

  [Fact]
  public async void JoiningLobbyCreatesShip()
  {
    var (provider, signalRMock) = getServiceProvider();
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
    var clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(provider)), "clientSupervisor");

    clientSupervisor.Tell(new CreateClientActor("tony", ""));
    await Task.Delay(100);
    clientSupervisor.Tell(new CreateLobby("tony"));
    await Task.Delay(100);
    clientSupervisor.Tell(new JoinLobby("tony", "tony"), probe.Ref);
    await Task.Delay(100);

    lobbySupervisor.Tell(new GetState("tony", "tony"), probe.Ref);
    var response = probe.ExpectMsg<GameStateSnapshot>();
    Ship ship = new()
    {
      Username = "tony",
      Direction = 45,
      Xpos = 50,
      Ypos = 50,
      Health = 50,
      Score = 0,
    };
    List<Ship> ships = [ship];
    response.Game.ships.Should().BeEquivalentTo(ships);

  }

  [Fact]
  public async void CreatingClientCanStartGame()
  {
    var (provider, signalRMock) = getServiceProvider();
    var probe = CreateTestProbe();
    var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
    var clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(provider)), "clientSupervisor");

    clientSupervisor.Tell(new CreateClientActor("tony", ""));
    await Task.Delay(100);

    clientSupervisor.Tell(new CreateLobby("tony"));
    await Task.Delay(100);

    clientSupervisor.Tell(new JoinLobby("tony", "tony"));
    await Task.Delay(100);

    clientSupervisor.Tell(new StartGame("tony"), probe.Ref);

    var response = probe.ExpectMsg<GameStateSnapshot>();

    response.Game.state.Should().Be(GameState.PLAYING);

  }

  //test 2 players join works


}