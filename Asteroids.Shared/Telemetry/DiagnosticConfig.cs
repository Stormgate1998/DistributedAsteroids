using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Asteroids.Shared.Telemetry;

public static class DiagnosticConfig
{
  public static string ServiceName = "je_asteroids";
  public static Meter Meter = new(ServiceName);
  public static Counter<int> tickCount = Meter.CreateCounter<int>("game.ticks");
}