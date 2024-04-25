using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Asteroids.Shared.Actors;
using Asteroids.Shared.GameObjects;
using Asteroids.Shared.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ActorTests;

public class StorageTests : TestKit
{
  private ServiceProvider getServiceProvider()
  {
    var services = new ServiceCollection();
    services.AddLogging(builder => builder.AddConsole());
    var raftServiceMock = Substitute.For<IRaftService>();
    services.AddSingleton(_ => raftServiceMock);

    var provider = services.BuildServiceProvider();
    return provider;
  }

  [Fact]
  public void StorageCanStoreSnapshotViaService()
  {
    // Arrange
    string lobbyName = "testLobby";
    string testKey = $"snapshot-of-{lobbyName}";
    Dictionary<string, GameStateObject> storage = [];

    var raftServiceMock = Substitute.For<IRaftService>();
    raftServiceMock.StoreGameSnapshot(Arg.Any<string>(), Arg.Any<GameStateObject>())
      .Returns(Task.CompletedTask)
      .AndDoes(callInfo =>
      {
        string key = callInfo.ArgAt<string>(0);
        GameStateObject snapshot = callInfo.ArgAt<GameStateObject>(1);

        storage[key] = snapshot;
      });

    var services = new ServiceCollection();
    services.AddLogging(builder => builder.AddConsole());
    services.AddSingleton(_ => raftServiceMock);

    var provider = services.BuildServiceProvider();
    var probe = CreateTestProbe();
    var storageActor = Sys.ActorOf(Props.Create(() => new StorageActor(provider)), "storageActor");

    var snapshot = new GameStateObject()
    {
      LobbyName = lobbyName,
      state = GameState.PLAYING,
      Ticks = 1024,
      ships = [],
      asteroids = [],
      bullets = [],
      particpatingUsers = []
    };

    // Act
    storageActor.Tell(new StoreState(testKey, snapshot), probe.Ref);
    probe.ExpectNoMsg();

    // Assert
    storage.Count.Should().Be(1);
    storage[testKey].LobbyName.Should().Be(lobbyName);
    storage[testKey].state.Should().Be(snapshot.state);
    storage[testKey].Ticks.Should().Be(snapshot.Ticks);
  }

  // [Fact]
  // public void StorageCanGetSnapshotFromRaft()
  // {

  // }
}