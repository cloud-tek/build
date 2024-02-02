# CloudTek.Build

Contains the shared build system based on NUKE.
The `CloudTek.Build` package is meant to be a reusable build system for a variety of projects which are to share behavior in a large multi-repo scenario.

> *Important*
>
> NUKE is a dotnet cli tool, which needs to be restored just like any other dotnet tool before use.
> Please ensure that you've run `dotnet tool restore` for local use


### Environment Variables & Parameters

> **Important**
>
>When invoking testing related targets, SmartBuild will propagate all Environment Variables to `dotnet test`. In order to ensure that your tests run correctly, please make sure that all required variable are present in your env (for instance in `~/.zshrc` or in your Powershell profile)

#### Integration testing

In order to execute all tests properly, please ensure that the
[cloud-tek/dev-env](https://github.com/cloud-tek/dev-env) has been properly intialized on your machine.

#### Parameter value binding

NUKE maps existing environment variables onto the parameters defined in the SmartBuild.

The following information is required to understand how NUKE parses parameter values and supplies them to a build:

- NUKE removes any non-letter and non-digit characters from the parameter name (or env var), when:
  - When a parameter is passed as a CLI argument
  - When a parameter is supplied as an env variable
- When attempting to map environment variables on to parameters:
  - NUKE uses `StringComparisonOptions.OrdinalIgnoreCase` to compare env var and parameter names
  - NUKE accepts `$env:PARAMETERNAME` or `$env:NUKE_PARAMETERNAME` environment variables
  - If a variable is specified in both formats, nuke will log a WRN: `Could not resolve {VariableName} since multiple values are provided` and will fail to bind the parameter value

NUKE's `ParameterService`'s env var binding logic can be found [here](https://github.com/nuke-build/nuke/blob/b94f4c110095b84e0173f44af5d2dc0579e9b2db/source/Nuke.Build/Execution/ParameterService.cs#L204C14-L204C14)

```csharp
/// <summary>
/// dotnet nuke --target Restore --skip-format-check
/// </summary>
[Parameter] public bool SkipFormatCheck { get; set; } = false;
```

The above parameter will accept `NUKE_SKIPFORMATCHECK` and `SKIPFORMATCHECK` (case insensitive) env vars as value sources. Only one may be used as described above.

## Supported file-system scenarios

### default

By default, `CloudTek.Build` assumes the following directory structure:

```
├─ build
├─ src
│   └── <Projects>
└─ test
    └── <TestProjects>
```

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    new static readonly Repository Repository = new ()
    {
        Artifacts = new Dictionary<string, ArtifactType>()
        {
            { "My.Project",                 ArtifactType.Service },
            { "My.Project.Client",          ArtifactType.Package },
        }
        .Select(x => new Artifact()
        {
            Type = x.Value,
            Path = RootDirectory / "src" / x.Key / "*.csproj"
        })
        .ToArray();
    };
}
```

### multi-module

`CloudTek.Build` supports any directory structure for sources:

```
├─ build
├─ module1
│   └── <Projects>
└─ module2
    └── <Projects>
```

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    new static readonly Repository Repository = new ()
    {
        Artifacts = new[]
        {
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module1" / "My.Module1.Project1" / "*.csproj" },
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module1" / "My.Module1.Project2" / "*.csproj" },
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module1" / "My.Module1.Project3" / "*.csproj" },
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module2" / "My.Module2.Project1" / "*.csproj" },
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module2" / "My.Module2.Project2" / "*.csproj" },
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "module2" / "My.Module2.Project3" / "*.csproj" }
        }
    }
```

## Usage

Create a NUKE build in your project. Add `CloudTek.Build` package.

Define a `SmartBuild` in one of the following ways:

### SmartBuild with a default versioning strategy

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    new static readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "src" / "My.Project" / "*.csproj" }
        }
    };
}
```

### SmartBuild with a GitVersion versioning strategy

#### Prerequisites

`SmartGitVersionBuild` required `GitVersion.Tool` to be installed.

```bash
dotnet tool restore
dotnet nuke :add-package GitVersion.Tool --version 5.8.2
```

#### Usage

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    new static readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "src" / "My.Project" / "*.csproj" }
        }
    };
}
```

The above definition is equivalent to:

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    new static readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "src" / "My.Project" / "*.csproj" }
        }
    };

    [GitVersion(Framework = "net5.0", NoFetch = true)]
    public GitVersion GitVersion { get; set; } = default!;
}
```

### SmartBuild Package Restoration

SmartBuild supports NuGet package manager, it is possible to extend it and add any other valid .NET package manager support.

```csharp
    public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
    {
        // (...)
    }
```

Restoring packages, checking if there are any (unpinned) BETA packages within the solution is achieved from the CLI level
irregardless of the used package manager type.

- `dotnet nuke --target Restore`

---

### SmartBuild Testing

There are multiple ways to start targets which conduct testing.

- `dotnet nuke --target UnitTests` - UnitTests & Compilation
- `dotnet nuke --target UnitTests --skip Compile` - UnitTests only without Compilation of the solution
- `dotnet nuke --target IntegrationTests` - IntegrationTests & Compilation
- `dotnet nuke --target IntegrationTests --skip Compile` - IntegrationTests only without Compilation of the solution
- `dotnet nuke --target Test` - All Tests, Checks & Compilation
- `dotnet nuke --target Test --skip-unit-tests <true | false>` - Integration Tests, Checks & Compilation
- `dotnet nuke --target Test --skip-integration-tests <true | false>` - Unit Tests, Checks & Compilation

#### Filtering

By default, build system supplies `dotnet test` with `--filter Flaky!=true`. This can be overriden and developers can provide their individual filters at SmartBuild level.

The filter syntax can be obtained from the official [documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=mstest)

```csharp
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);

    public Build() : base(Repository)
    { }

    protected override string TestFilter { get; init; } = "Flaky!=true|TestCategory=CategoryA";

    new static readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Path = RootDirectory / "src" / "My.Project" / "*.csproj" }
        }
    };
}
```

#### CodeCoverage

SmartBuild can collect code coverage information from your project.

- `dotnet nuke --target UnitTests --collect-coverage <true | false>`

Enabling the `--collect-coverage` will cause the SmartBuild to run `dotnet add package coverlet.msbuild --version 6.0.0` against all test assemblies and add additional cmdline arguments to `dotnet test` which will cause coverage information collection.

The process is designed to collect coverage from multiple test assemblies.

Intermediate results are stored in `test/coverage/coverage.temp.json`.
Final coverage results are stored in `test/coverage/coverage.xml` in Cobertura format.

> **Warning**
>
> When code coverage collection is enabled, there needs to be at least one test covering the code in the project in order for `dotnet test` to complete successfully

### SmartBuild Checks

The `RunChecks` target can be used in order to execute all standard checkswithin the solution.

- `dotnet nuke --target RunChecks`

Checks can be skipped using dedicated boolean flags. The flags can be combined:

- `--skip-beta-check`: skips BETA packages check
- `--skip-outdated-check`: skips OUTDATED packages check
- `--skip-format-check`: skips the format check
- `--skip-commitlint-check`: skips the commitlint check

Example:

- `dotnet nuke --target RunChecks --skip-beta-check`

#### CommitLintCheck

The `CommitLintCheck` target can be used in order to validate commit messages server-side using [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) and [husky](https://alirezanet.github.io/Husky.Net/)

- `dotnet nuke --target CommitLintCheck`

The target will run a dependend `HuskyInstall` target which will execute:

- `dotnet husky install`

Requires `.git` folder to be present.

#### FormatCheck

The `FormatCheck` target can be used in order to execute the following dotnet CLI commands against the solution using a single NUKE target:

- `dotnet format --no-verify`
- `dotnet format analyzers --no-verify`

The target will cause no side-effects.

- `dotnet nuke --target FormatCheck`
- `dotnet nuke --target FormatCheck --skip-format-check` - target will be skipped
- `dotnet nuke --target FormatCheck --skip-format-check true` - target will be skipped
- `dotnet nuke --target FormatCheck --skip-format-check false` - target will not be skipped

##### Format

The `Format` target works in the same way as `FormatCheck`, except for the fact that the `--no-verify` flags are NOT being added to the dotnet CLI and the target WILL CAUSE SIDE-EFFECTS.

#### PackageBetaCheck

The `PackageBetaCheck` target can be used in order to check for BETA packages within the solution irregardless of the chosen package manager. The target needs to be run **AFTER** the `Restore` target.

- `dotnet nuke --target PackageBetaCheck`
- `dotnet nuke --target PackageBetaCheck --skip-beta-check` - target will be skipped
- `dotnet nuke --target PackageBetaCheck --skip-beta-check true` - target will be skipped
- `dotnet nuke --target PackageBetaCheck --skip-beta-check false` - target will not be skipped

The `PackageBetaCheck` will cause 1 additional target to be executed before `Restore`:

- `BuildDependencyTree`

The `PackagesBetaCheck` will check the dependency tree for any BETA packages that are unpinned in either `.csproj` using the `[]` syntax (nuget) or in the global `paket.dependencies` file (paket).
The target will cause `dotnet list package` to execute and gather all information about semantically versioned pre-release packages.

#### PackageOutdatedCheck

The `PackageOutdatedCheck` target can be used in order to check for BETA packages within the solution irregardless of the chosen package manager. The target needs to be run **AFTER** the `Restore` target.

- `dotnet nuke --target PackageOutdatedCheck`
- `dotnet nuke --target PackageOutdatedCheck --skip-outdated-check` - target will be skipped
- `dotnet nuke --target PackageOutdatedCheck --skip-outdated-check true` - target will be skipped
- `dotnet nuke --target PackageOutdatedCheck --skip-outdated-check false` - target will not be skipped

The `PackageOutdatedCheck` will cause 1 additional target to be executed before `Restore`:

- `BuildDependencyTree`

The `PackageOutdatedCheck` will check the dependency tree for any OUTDATED packages that are unpinned in either `.csproj` using the `[]` syntax (nuget) or in the global `paket.dependencies` file (paket).
The target will cause `dotnet list package --outdated` to execute and gather all packages' `Latest` available versions to compare with their `Resolved` versions.

##### Building the dependency tree

 `BuildDependencyTree` - will analyze the solution and create a map of projects' packages including their versions and pinned state. The target will be run before `PackageBetaCheck` or `PackageOutdatedCheck` targets.