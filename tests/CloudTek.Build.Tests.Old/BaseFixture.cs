using System.Globalization;
using Serilog;
using Xunit.Abstractions;

namespace CloudTek.Build.Tests;

public abstract class BaseFixture
{
  protected readonly ITestOutputHelper Output;

  protected BaseFixture(ITestOutputHelper output)
  {
    Output = output;

    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.TestOutput(Output, Serilog.Events.LogEventLevel.Verbose, formatProvider: CultureInfo.InvariantCulture)
      .CreateLogger();
  }
}