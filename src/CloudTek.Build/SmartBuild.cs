using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CloudTek.Build.Extensions;
using CloudTek.Build.Packaging;
using CloudTek.Build.Versioning;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

namespace CloudTek.Build
{
  /// <summary>
  /// SmartBuild with versioning strategy
  /// </summary>
  /// <typeparam name="TVersioningStrategy"></typeparam>
  public abstract class SmartBuild<TVersioningStrategy> : SmartBuild
    where TVersioningStrategy : VersioningStrategy, new()
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    protected SmartBuild() : base(new PackageManager(), new TVersioningStrategy())
    {
    }
  }

  /// <summary>
  /// SmartBuild is the NUKE Build wrapper, providing all of the default functionalities
  /// </summary>
  public abstract partial class SmartBuild : NukeBuild
  {
    /// <summary>
    /// Package manager
    /// </summary>
    protected readonly PackageManager PackageManager;

    /// <summary>
    /// Versioning strategy for SmartBuild
    /// </summary>
    protected readonly VersioningStrategy VersioningStrategy;

    private Solution? _solution;

    /// <summary>
    /// Internal flag indicating whether the solution has been built. Will result in appending --no-build to all subsequent dotnet CLI invocations
    /// </summary>
    internal bool SolutionBuilt;

    /// <summary>
    /// Internal flag indicating whether the solution has been restored. Will result in appending --no-restore to all subsequent dotnet CLI invocations
    /// </summary>
    internal bool SolutionRestored;

    /// <summary>
    /// Default constructor
    /// </summary>
    protected SmartBuild(
      PackageManager packageManager,
      VersioningStrategy versioningStrategy)
    {
      Environment.SetEnvironmentVariable("NUKE_TELEMETRY_OPTOUT", "true");
      Environment.SetEnvironmentVariable("NOLOGO", "true");

      PackageManager = packageManager;
      VersioningStrategy = versioningStrategy;
      EnvironmentVariables = new ReadOnlyDictionary<string, string?>(
        Environment.GetEnvironmentVariables()
          .OfType<DictionaryEntry>()
          .ToDictionary(entry => (string)entry.Key, entry => (string?)entry.Value));
    }

    /// <summary>
    /// Global tool location on the dotnet tool
    /// </summary>
    public static AbsolutePath GlobalToolDirectory => EnvironmentInfo.SpecialFolder(SpecialFolders.UserProfile) / ".cloud-tek.build";

    /// <summary>
    /// Environment variables
    /// </summary>
    protected IReadOnlyDictionary<string, string?> EnvironmentVariables { get; init; }

    /// <summary>
    /// Filter used for dotnet test
    /// </summary>
    [Parameter(
      "Test filter. Default is 'Flaky!=true'")]
    public virtual string TestFilter { get; init; } = "Flaky!=true";

    /// <summary>
    /// Space-separated list of packages to be checked during pre-build checks
    /// </summary>
    [Parameter(
      "Space separated strings for outdated and beta checks to narrow search. Filter works as contains function. If any string match - package is included in results. Default is 'CloudTek Core'.")]
    public virtual string PackagesFilter { get; init; } =
      "CloudTek Core";

    /// <summary>
    /// Build configuration (Debug | Release). Defaults to Debug for local builds.
    /// </summary>
    [Parameter("Configuration for dotnet commands. Default is 'Debug' (local) or 'Release' (server)")]
    public virtual Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    /// <summary>
    /// CI Build number
    /// </summary>
    [Parameter("A buildNumber for beta suffix calculation. Default is empty")]
    public virtual string BuildNumber { get; init; } = string.Empty;

    /// <summary>
    /// Flag indicating whether code coverage is to be enabled
    /// </summary>
    [Parameter("Enabling code coverage collection. Default is false")]
    public virtual bool CollectCoverage { get; set; }

    /// <summary>
    /// Solution information for SmartBuild
    /// </summary>
    public Solution Solution => _solution ??= GetSolution();

    /// <summary>
    /// Repository information for SmartBuild
    /// </summary>
    public Repository Repository { get; private set; } = default!;

    /// <summary>
    /// GitRepository information for SmartBuild
    /// </summary>
    [GitRepository] public GitRepository? GitRepository { get; private set; }

    /// <summary>
    /// dotnet nuke --target All
    /// </summary>
    protected virtual Target All => _ => _
      .DependsOn(Compile, Test, Pack, Publish)
      .Executes(
        () =>
        {
          Log.Logger.Information($"All targets executed");
        });

    /// <summary>
    /// Executed whenever a build is created. Ensures the build is initialized and the logo is displayed.
    /// </summary>
    protected override void OnBuildCreated()
    {
      Console.WriteLine();
      Console.WriteLine("〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰");
      Console.WriteLine(
        " \u2588\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2557      \u2588\u2588\u2588\u2588\u2588\u2588\u2557 \u2588\u2588\u2557   \u2588\u2588\u2557\u2588\u2588\u2588\u2588\u2588\u2588\u2557    \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2557  \u2588\u2588\u2557   \u2588\u2588\u2557 \u2588\u2588\u2588\u2588\u2588\u2588\u2557 \n\u2588\u2588\u2554\u2550\u2550\u2550\u2550\u255d\u2588\u2588\u2551     \u2588\u2588\u2554\u2550\u2550\u2550\u2588\u2588\u2557\u2588\u2588\u2551   \u2588\u2588\u2551\u2588\u2588\u2554\u2550\u2550\u2588\u2588\u2557   \u255a\u2550\u2550\u2588\u2588\u2554\u2550\u2550\u255d\u2588\u2588\u2554\u2550\u2550\u2550\u2550\u255d\u2588\u2588\u2551 \u2588\u2588\u2554\u255d   \u2588\u2588\u2551\u2588\u2588\u2554\u2550\u2550\u2550\u2588\u2588\u2557\n\u2588\u2588\u2551     \u2588\u2588\u2551     \u2588\u2588\u2551   \u2588\u2588\u2551\u2588\u2588\u2551   \u2588\u2588\u2551\u2588\u2588\u2551  \u2588\u2588\u2551\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2551   \u2588\u2588\u2588\u2588\u2588\u2557  \u2588\u2588\u2588\u2588\u2588\u2554\u255d    \u2588\u2588\u2551\u2588\u2588\u2551   \u2588\u2588\u2551\n\u2588\u2588\u2551     \u2588\u2588\u2551     \u2588\u2588\u2551   \u2588\u2588\u2551\u2588\u2588\u2551   \u2588\u2588\u2551\u2588\u2588\u2551  \u2588\u2588\u2551\u255a\u2550\u2550\u2550\u2550\u255d\u2588\u2588\u2551   \u2588\u2588\u2554\u2550\u2550\u255d  \u2588\u2588\u2554\u2550\u2588\u2588\u2557    \u2588\u2588\u2551\u2588\u2588\u2551   \u2588\u2588\u2551\n\u255a\u2588\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2557\u255a\u2588\u2588\u2588\u2588\u2588\u2588\u2554\u255d\u255a\u2588\u2588\u2588\u2588\u2588\u2588\u2554\u255d\u2588\u2588\u2588\u2588\u2588\u2588\u2554\u255d      \u2588\u2588\u2551   \u2588\u2588\u2588\u2588\u2588\u2588\u2588\u2557\u2588\u2588\u2551  \u2588\u2588\u2557\u2588\u2588\u2557\u2588\u2588\u2551\u255a\u2588\u2588\u2588\u2588\u2588\u2588\u2554\u255d\n \u255a\u2550\u2550\u2550\u2550\u2550\u255d\u255a\u2550\u2550\u2550\u2550\u2550\u2550\u255d \u255a\u2550\u2550\u2550\u2550\u2550\u255d  \u255a\u2550\u2550\u2550\u2550\u2550\u255d \u255a\u2550\u2550\u2550\u2550\u2550\u255d       \u255a\u2550\u255d   \u255a\u2550\u2550\u2550\u2550\u2550\u2550\u255d\u255a\u2550\u255d  \u255a\u2550\u255d\u255a\u2550\u255d\u255a\u2550\u255d \u255a\u2550\u2550\u2550\u2550\u2550\u255d ");

      Console.WriteLine("Go here to learn about CloudTek.Build");
      Console.WriteLine(
        "https://github.com/cloud-tek/build");

      Console.WriteLine("〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰〰");

      Repository = new Repository(Solution, GitRepository);
      InitializeTelemetry();

      base.OnBuildCreated();
    }

    /// <summary>
    /// Executes whenever a build is initialized. Starts measurements
    /// </summary>
    protected override void OnBuildInitialized()
    {
      _stopwatches.Add("Total", Stopwatch.StartNew());
      SkipTargetsFromEnvVariable();
      base.OnBuildInitialized();
      TargetDefinitionExtensions.PrintTargetsToSkip();
      if (GitRepository == null)
      {
        Log.Warning("Didn't detect git repository!");
      }
    }

    private void SkipTargetsFromEnvVariable()
    {
      var nukeSkip = EnvironmentInfo.Variables.GetValueOrDefault("NUKE_SKIP");
      var targetsToSkip = nukeSkip?.Split("+") ?? [];
      foreach (var target in ExecutionPlan.Where(p => targetsToSkip.Contains(p.Name)).ToList())
      {
        target.StaticConditions.Add(
          new ValueTuple<string, Func<bool>>("Skipped due to NUKE_SKIP variable", () => false));
      }
    }

    /// <summary>
    /// Post-build duration report and cleanUp
    /// </summary>
    protected override void OnBuildFinished()
    {
      ReportDuration(
        "Total",
        Repository.Name,
        IsSuccessful,
        _stopwatches["Total"].ElapsedMilliseconds);

      LoggerFactory?.Dispose();
      Meter?.Dispose();
      MetricsProvider?.Dispose();

      base.OnBuildFinished();
    }

    private static Solution GetSolution()
    {
      EnsureIsPackableDisabledByDefault();

      var paths = RootDirectory.GlobFiles("*sln");
      if (paths.Count == 0)
        throw new InvalidOperationException("Didn't find any solution file - please, create one :)");
      if (paths.Count > 1)
      {
        throw new InvalidOperationException(
          "There is more than one solution - please make sure that you run the SmartBuild in a folder with only one solution. Any additional solutions are to be moved to separate folders");
      }

      return SolutionModelTasks.ParseSolution(paths.Single());
    }

    private static void EnsureIsPackableDisabledByDefault()
    {
      var directoryBuildPropsPath = Path.Combine(Directory.GetCurrentDirectory(), "Directory.Build.props");

      if (!File.Exists(directoryBuildPropsPath))
      {
        // File does not exist, create it
        var content = @"<Project>
                              <PropertyGroup>
                                <IsPackable>false</IsPackable>
                              </PropertyGroup>
                            </Project>";

        File.WriteAllText(directoryBuildPropsPath, content);
      }
      else
      {
        var doc = XDocument.Load(directoryBuildPropsPath);

        var root = doc.Root;
        if (root == null)
        {
          // File is invalid, recreate it
          root = new XElement("Project");
          doc = new XDocument(root);
        }

        var isPackable = root.Descendants("IsPackable").FirstOrDefault()?.Value;
        var disablePackableCheck = root.Descendants("DisablePackableCheckInRoot").FirstOrDefault()?.Value;

        if (disablePackableCheck?.ToLowerInvariant() == "true")
          return;

        if (isPackable == null)
        {
          // Add PropertyGroup if needed
          var propertyGroup = root.Element("PropertyGroup");
          if (propertyGroup == null)
          {
            propertyGroup = new XElement("PropertyGroup");
            root.Add(propertyGroup);
          }

          propertyGroup.SetElementValue("IsPackable", "false");
        }

        var settings = new XmlWriterSettings
        {
          OmitXmlDeclaration = true,
          Indent = true,
          Encoding = new UTF8Encoding(false)
        };
        using var xw = XmlWriter.Create(directoryBuildPropsPath, settings);
        doc.Save(xw);
      }
    }
  }
}