using Akka.Actor;
using FluentAssertions;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
using Asteroids.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Akka.TestKit;

namespace ActorTests;

public class ClientTests : TestKit
{
  private (ServiceProvider provider, IHubService realTimeMock) getServiceProvider()
  {
    var services = new ServiceCollection();
    services.AddLogging(builder => builder.AddConsole());
    var realTimeMock = Substitute.For<IHubService>();
    services.AddSingleton(_ => realTimeMock);
    var raftServiceMock = Substitute.For<IRaftService>();
    services.AddSingleton(_ => raftServiceMock);

    var provider = services.BuildServiceProvider();
    return (provider, realTimeMock);
  }

  private void CreateClientSupervisor(out TestProbe probe, out IHubService signalRMock, out IActorRef clientSupervisor, out IActorRef lobbySupervisor)
  {
    probe = CreateTestProbe();
    (var provider, signalRMock) = getServiceProvider();

    var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor(provider)), "storageActor");

    var storageProbe = CreateTestProbe();
    lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageActor)), "lobbySupervisor");
    IActorRef newOne = lobbySupervisor;
    clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(newOne, provider)), "clientSupervisor");
  }

  [Fact]
  public async void ClientSupervisorCanCreateClient()
  {
    CreateClientSupervisor(out TestProbe probe, out IHubService signalRMock, out IActorRef clientSupervisor, out IActorRef lobbySupervisor);
    clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);

    await Task.Delay(100);
    await signalRMock.Received().SendClientState(Arg.Any<string>(), Arg.Is<ClientState>(s => s == ClientState.NoLobby));
  }

  private void CreateClientActor(string name, string connectionId, out TestProbe probe, out IActorRef lobbySupervisor, out IActorRef client)
  {
    var (provider, signalRMock) = getServiceProvider();
    var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor(provider)), "storageActor");

    probe = CreateTestProbe();
    var storageProbe = CreateTestProbe();
    lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageActor)), "lobbySupervisor");
    client = Sys.ActorOf(Props.Create<ClientActor>(name, connectionId, lobbySupervisor, provider), name);
  }



  [Fact]
  public async void ClientSupervisorCanNotCreateDuplicateClient()
  {
    CreateClientSupervisor(out TestProbe probe, out IHubService signalRMock, out IActorRef clientSupervisor, out IActorRef lobbySupervisor);

    clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);
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
    CreateClientActor("tony", "fake id", out TestProbe probe, out IActorRef lobbySupervisor, out IActorRef client);

    client.Tell(new CreateLobby("tony"));
    await Task.Delay(100);


    lobbySupervisor.Tell(new GetLobbies("intentionally left blank"), probe.Ref);
    var response = probe.ExpectMsg<GetLobbiesResponse>();

    List<string> TestList = ["tony"];
    response.Lobbies.Should().BeEquivalentTo(TestList);
  }



  // [Fact]
  // public async void ClientCanJoinExisitingLobby()
  // {
  //   var probe = CreateTestProbe();
  //   var (provider, signalRMock) = getServiceProvider();

  //   var storageProbe = CreateTestProbe();
  //   var lobbySupervisor = Sys.ActorOf(Props.Create(() => new LobbySupervisorActor(storageProbe)), "lobbySupervisor");
  //   var client = Sys.ActorOf(Props.Create<ClientActor>("tony", "connectionId", lobbySupervisor, provider), "tony");

  //   lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
  //   probe.ExpectMsg<CreateLobbyResponse>();

  //   client.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
  //   await Task.Delay(100);

  //   client.Tell(new GetClientState(), probe.Ref);
  //   probe.ExpectMsg<GetClientStateResponse>(response => response.State == ClientState.InLobby);
  // }


  [Fact]
  public async void ClientCannotJoinNonExisitingLobby()
  {
    CreateClientActor("tony", "fake id", out TestProbe probe, out IActorRef lobbySupervisor, out IActorRef client);


    client.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
    await Task.Delay(100);

    client.Tell(new GetClientState(), probe.Ref);
    probe.ExpectMsg<GetClientStateResponse>(response => response.State == ClientState.NoLobby);
  }


  // [Fact]
  // public void ClientSupervisorCanJoinLobby()
  // {
  //   var probe = CreateTestProbe();
  //   var (provider, signalRMock) = getServiceProvider();

  //   var lobbySupervisor = Sys.ActorOf(Props.Create<LobbySupervisorActor>(), "lobbySupervisor");
  //   var clientSupervisor = Sys.ActorOf(Props.Create(() => new ClientSupervisorActor(provider)), "clientSupervisor");

  //   clientSupervisor.Tell(new CreateClientActor("tony", "connectionId"), probe.Ref);
  //   probe.ExpectMsg<CreateClientActorResponse>();

  //   lobbySupervisor.Tell(new CreateLobby("testLobby"), probe.Ref);
  //   probe.ExpectMsg<CreateLobbyResponse>();

  //   clientSupervisor.Tell(new JoinLobby("testLobby", "tony"), probe.Ref);
  //   probe.ExpectMsg<JoinLobbyResponse>();
  // }

  [Fact]
  public async void JoiningLobbyCreatesShip()
  {

    CreateClientSupervisor(out TestProbe probe, out IHubService signalRMock, out IActorRef clientSupervisor, out IActorRef lobbySupervisor);

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
      Location = new(100, 100),
      Health = 200,
      Score = 0,
    };
    List<Ship> ships = [ship];
    response.Game.ships.Should().BeEquivalentTo(ships);
  }

  // [Fact]
  // public async void CreatingClientCanStartGame()
  // {
  //   CreateClientSupervisor(out TestProbe probe, out IHubService signalRMock, out IActorRef clientSupervisor, out IActorRef lobbySupervisor);

  //   clientSupervisor.Tell(new CreateClientActor("tony", ""));
  //   await Task.Delay(100);

  //   clientSupervisor.Tell(new CreateLobby("tony"));
  //   await Task.Delay(100);

  //   clientSupervisor.Tell(new JoinLobby("tony", "tony"));
  //   await Task.Delay(100);

  //   clientSupervisor.Tell(new StartGame("tony"));
  //   await Task.Delay(100);
  //   lobbySupervisor.Tell(new GetState("tony", "tony"), probe.Ref);

  //   var response = probe.ExpectMsg<GameStateSnapshot>();

  //   response.Game.state.Should().Be(GameState.PLAYING);

  // }
}