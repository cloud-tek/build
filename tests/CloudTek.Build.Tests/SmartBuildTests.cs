using System.Globalization;
using System.Reflection;
using FluentAssertions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace CloudTek.Build.Tests;

public class SmartBuildTests : TestBuildFixture
{
  private readonly ITestOutputHelper _output;
  public SmartBuildTests(ITestOutputHelper output)
  {
    _output = output;

    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose, formatProvider: CultureInfo.InvariantCulture)
      .CreateLogger();
  }
}