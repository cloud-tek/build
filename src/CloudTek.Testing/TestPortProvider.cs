using System.Net;
using System.Net.NetworkInformation;

namespace CloudTek.Testing;

/// <summary>
///   Provides a free port for testing.
/// </summary>
public static class TestPortProvider
{
  /// <summary>
  ///   Provides an available port, for testing, starting with the startingPort
  /// </summary>
  /// <param name="startingPort"></param>
  /// <param name="port"></param>
  /// <returns>Disposable port scope</returns>
  public static IDisposable GetAvailableServicePortScope(ushort startingPort, out ushort port)
  {
    port = GetAvailablePort(startingPort);

    return EnvironmentVariableScope.Create(
      "ASPNETCORE_URLS",
      $"http://localhost:{port}");
  }

  /// <summary>
  ///   Provides an available port, for testing, starting with the startingPort
  /// </summary>
  /// <param name="startingPort"></param>
  /// <returns>Port number</returns>
  public static ushort GetAvailablePort(ushort startingPort)
  {
    var portArray = new List<ushort>();

    var properties = IPGlobalProperties.GetIPGlobalProperties();

    if (OperatingSystem.IsWindows())
    {
      portArray.AddRange(
        GetPorts(
          () => properties.GetActiveTcpConnections().Select(x => x.LocalEndPoint),
          startingPort));
      portArray.AddRange(GetPorts(() => properties.GetActiveTcpListeners(), startingPort));
      portArray.AddRange(GetPorts(() => properties.GetActiveUdpListeners(), startingPort));
    }
    else if (!OperatingSystem.IsWindows())
    {
      portArray.AddRange(
        GetPorts(
          () => properties.GetActiveTcpConnections().Select(x => x.LocalEndPoint),
          startingPort,
          true));
      portArray.AddRange(
        GetPorts(
          () => properties.GetActiveTcpConnections()
            .Where(x => x.State == TcpState.Listen)
            .Select(x => x.LocalEndPoint),
          startingPort,
          true));
      portArray.AddRange(GetPorts(() => properties.GetActiveUdpListeners(), startingPort, true));
    }

    portArray = portArray.Distinct().ToList();
    portArray.Sort();

    for (var i = startingPort; i < ushort.MaxValue; i++)
    {
      if (!portArray.Contains(i))
      {
        return Convert.ToUInt16(i);
      }
    }

    return 0;
  }

  private static IEnumerable<ushort> GetPorts(
    Func<IEnumerable<IPEndPoint>> endpointSelector,
    ushort startingPort,
    bool invertEndianness = false)
  {
    return endpointSelector()
      .Where(x => x.Port >= startingPort)
      .Select(x => invertEndianness ? InvertEndianness(Convert.ToUInt16(x.Port)) : Convert.ToUInt16(x.Port));
  }

  /// <summary>
  ///   Inverts the endianness of a ushort
  /// </summary>
  /// <param name="value"></param>
  /// <returns>The ushort value with endianness inverted</returns>
  public static ushort InvertEndianness(ushort value)
  {
    return (ushort)(((value & 0xFFU) << 8) | ((value & 0xFF00U) >> 8));
  }
}