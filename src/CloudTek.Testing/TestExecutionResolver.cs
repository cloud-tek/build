#pragma warning disable MA0099
using System.Text;

namespace CloudTek.Testing;

internal static class TestExecutionResolver
{
  internal static IDictionary<Execute, string> SkipReasonForExecute = new Dictionary<Execute, string>
  {
    { Execute.InGithubActions, "Test to be executed only in Github Actions." },
    { Execute.InAzureDevOps, "Test to be executed only in Azure DevOps." },
    { Execute.InContainer, "Test to be executed only in a .NET Core Container." },
    { Execute.InDebug, "Test to be executed only in DEBUG configuration." }
  };

  internal static IDictionary<On, string> SkipReasonForOn = new Dictionary<On, string>
  {
    { On.Windows, "Test to be executed on Windows." },
    { On.Linux, "Test to be executed on Linux." },
    { On.MacOS, "Test to be executed on MacOS." }
  };

  private static readonly string[] RequiredGitHubActionsEnvVariables = ["GITHUB_ACTIONS"];

  private static readonly string[] RequiredAzureDevOpsEnvVariables = ["AGENT_ID", "BUILD_BUILDID"];

  public static string? Resolve(Execute execute, On on, IEnumerable<string>? environment = null)
  {
    var errors = new[] { Resolve(execute), Resolve(on), environment != null ? Resolve(environment) : null };

    var count = errors.Count(e => e != null);

    if (count > 1)
    {
      var result = new StringBuilder();

      errors.Where(e => e != null).ForEach(
        e =>
        {
          result.Append(e);

          if (e != errors.Last(e => e != null))
          {
            result.Append(" & ");
          }
        });

      return result.ToString();
    }

    return count == 1 ? errors.Single(e => e != null) : null;
  }

  internal static string? Resolve(Execute execute)
  {
    if ((Execute.InGithubActions & execute) != 0)
    {
      var err = ValidateEnvVariablesExists(
        RequiredGitHubActionsEnvVariables,
        () => SkipReasonForExecute[Execute.InGithubActions]);
      if (err != null)
      {
        return err;
      }
    }

    if ((Execute.InAzureDevOps & execute) != 0)
    {
      var err = ValidateEnvVariablesExists(
        RequiredAzureDevOpsEnvVariables,
        () => SkipReasonForExecute[Execute.InAzureDevOps]);
      if (err != null)
      {
        return err;
      }
    }

    if ((Execute.InContainer & execute) != 0)
    {
      var err = ValidateEnvVariableValue(
        "DOTNET_RUNNING_IN_CONTAINER",
        "true",
        () => SkipReasonForExecute[Execute.InContainer]);

      if (err != null)
      {
        return err;
      }
    }

    if ((Execute.InDebug & execute) != 0)
    {
#if DEBUG
#else
      return SkipReasonForExecute[Execute.InDebug];
#endif
    }

    return null;
  }

  internal static string? Resolve(On on)
  {
    return (On.Windows & on) == On.Windows && !OperatingSystem.IsWindows()
      ? SkipReasonForOn[On.Windows]
      : (On.Linux & on) == On.Linux && !OperatingSystem.IsLinux()
      ? SkipReasonForOn[On.Linux]
      : (On.MacOS & on) == On.MacOS && !OperatingSystem.IsMacOS() ? SkipReasonForOn[On.MacOS] : null;
  }

  internal static string? Resolve(IEnumerable<string> environment)
  {
    return ValidateEnvVariablesExists(environment, () => "A required environment variable is missing");
  }

  private static string? ValidateEnvVariablesExists(IEnumerable<string> names, Func<string> errorSelector)
  {
    foreach (var name in names)
    {
      var val = Environment.GetEnvironmentVariable(name);
      if (string.IsNullOrEmpty(val))
      {
        return errorSelector();
      }
    }

    return null;
  }

  private static string? ValidateEnvVariableValue(string name, string value, Func<string> errorSelector)
  {
    var val = Environment.GetEnvironmentVariable(name);
    return string.IsNullOrEmpty(val) || val.Equals(value, StringComparison.OrdinalIgnoreCase)
      ? errorSelector()
      : null;
  }

  private static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
  {
    _ = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
    foreach (var e in enumerable)
    {
      action(e);
    }
  }
}
#pragma warning restore MA0099