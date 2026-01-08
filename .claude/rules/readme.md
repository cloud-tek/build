# CloudTek.Build Toolset Guide

## Overview

This project uses the **CloudTek.Build** toolset - a standardized build and testing framework consisting of three NuGet packages:

1. **CloudTek.Build** - NUKE-based build automation with quality checks
2. **CloudTek.Testing** - XUnit extensions for conditional and categorized testing
3. **CloudTek.Git** - Conventional commit validation via Husky.NET

These packages provide a consistent development workflow across all CloudTek repositories.

## Quick Start

### Prerequisites

```bash
# Restore dotnet tools (NUKE, GitVersion, Husky, etc.)
dotnet tool restore

# Restore NuGet packages
dotnet nuke --target Restore
```

### Common Commands

```bash
# Run all checks and tests
dotnet nuke --target Test

# Run only unit tests
dotnet nuke --target UnitTests

# Run quality checks (format, commits, package validation)
dotnet nuke --target RunChecks

# Build and package
dotnet nuke --target Pack

# See all available targets
dotnet nuke --help
```

## Build Targets

### Core Workflow Targets

- **`All`** - Complete build pipeline: Compile → Test → Pack → Publish
- **`Compile`** - Build the solution (depends: Restore)
- **`Clean`** - Remove build outputs and artifacts
- **`Restore`** - Restore NuGet packages

### Testing Targets

- **`Test`** - Run all tests (Unit + Integration + Quality Checks)
- **`UnitTests`** - Execute unit tests only
- **`IntegrationTests`** - Execute integration tests only

**Common Usage:**
```bash
# Full test suite
dotnet nuke --target Test

# Unit tests with code coverage
dotnet nuke --target UnitTests --collect-coverage true

# Integration tests only, skip compilation
dotnet nuke --target IntegrationTests --skip Compile

# Skip specific test types
dotnet nuke --target Test --skip IntegrationTests
```

### Quality Check Targets

- **`RunChecks`** - Execute all quality checks
- **`CommitCheck`** - Validate commit messages (conventional commits)
- **`FormatCheck`** - Verify code formatting (read-only)
- **`Format`** - Apply code formatting (modifies files)
- **`FormatAnalyzersCheck`** - Check analyzer-based formatting
- **`BetaCheck`** - Detect unpinned pre-release packages
- **`OutdatedCheck`** - Detect outdated packages
- **`VulnerabilityCheck`** - Scan for vulnerable packages

**Common Usage:**
```bash
# All checks
dotnet nuke --target RunChecks

# Skip specific checks
dotnet nuke --target RunChecks --skip BetaCheck --skip OutdatedCheck

# Apply formatting (MODIFIES FILES)
dotnet nuke --target Format
```

### Packaging Targets

- **`Pack`** - Create NuGet packages with versioning
- **`Publish`** / **`PublishArtifacts`** - Publish service artifacts
- **`PublishTests`** - Publish test artifacts

### Package Management Targets

- **`UpdateOutdated`** - Update outdated packages
- **`BuildDependencyTree`** - Analyze solution dependencies

## Build Parameters

### Boolean Flags

```bash
--collect-coverage       # Enable code coverage collection (default: false)
--publish-as-zip         # Create ZIP archives of published artifacts (default: false)
--publish-in-parallel    # Publish projects in parallel (default: false)
```

### String Parameters

```bash
--configuration <Debug|Release>    # Build configuration (default: Debug local, Release CI)
--test-filter <expression>         # Test filter (default: "Flaky!=true")
--packages-filter <pattern>        # Filter packages for checks (e.g., "Microsoft")
--runtime <rid>                    # Target runtime (e.g., linux-x64, win-x64)
--version <version>                # Override package version (GitVersion builds)
```

### Skipping Targets

Via CLI:
```bash
dotnet nuke --target Test --skip UnitTests
dotnet nuke --target RunChecks --skip BetaCheck --skip CommitCheck
```

Via environment variable:
```bash
export NUKE_SKIP="BetaCheck;OutdatedCheck"
dotnet nuke --target RunChecks
```

## Testing with CloudTek.Testing

### Test Categories

Decorate tests with category attributes to enable selective execution:

```csharp
using CloudTek.Testing;

[UnitTest]        // Fast, isolated tests
[Fact]
public void MyUnitTest() { }

[IntegrationTest] // Tests requiring external dependencies
[Fact]
public void MyIntegrationTest() { }

[SmokeTest]       // Basic health checks
[Fact]
public void MySmokeTest() { }

[ModuleTest]      // Module-level integration
[Fact]
public void MyModuleTest() { }

[SystemTest]      // End-to-end system tests
[Fact]
public void MySystemTest() { }
```

Build targets filter by category:
- `UnitTests` → `Category=Unit`
- `IntegrationTests` → `Category=Integration`

### Platform-Specific Tests

```csharp
using CloudTek.Testing;

// Run only on Linux
[UnitTest]
[SmartFact(On.Linux)]
public void LinuxOnlyTest() { }

// Run only on Windows
[UnitTest]
[SmartFact(On.Windows)]
public void WindowsOnlyTest() { }

// Run only on macOS
[UnitTest]
[SmartFact(On.MacOS)]
public void MacOSOnlyTest() { }

// Run on Unix systems (Linux or macOS)
[UnitTest]
[SmartFact(On.Linux | On.MacOS)]
public void UnixOnlyTest() { }
```

### Environment-Specific Tests

```csharp
// Run only in CI (GitHub Actions)
[IntegrationTest]
[SmartFact(On.All, Execute.InGithubActions)]
public void CIOnlyTest() { }

// Run only in Azure DevOps
[IntegrationTest]
[SmartFact(On.All, Execute.InAzureDevOps)]
public void AzureDevOpsTest() { }

// Run only in containers
[IntegrationTest]
[SmartFact(On.All, Execute.InContainer)]
public void ContainerTest() { }

// Run only in Debug builds
[UnitTest]
[SmartFact(On.All, Execute.InDebug)]
public void DebugOnlyTest() { }
```

### Parameterized Tests (SmartTheory)

```csharp
[UnitTest]
[SmartTheory(On.Linux)]
[InlineData(1)]
[InlineData(2)]
[InlineData(3)]
public void ParameterizedTest(int value) { }
```

### Feature Tracking

```csharp
[Feature("JIRA-12345")]
[UnitTest]
[SmartFact(On.All)]
public void FeatureTest() { }
```

### Test Filtering

Override default filter in your build:

```csharp
// In build/Build.cs
public class Build : SmartBuild<VersioningStrategy.Default>
{
    protected override string TestFilter { get; init; } = "Flaky!=true|Priority=High";
}
```

Or via CLI:
```bash
dotnet nuke --target UnitTests --test-filter "Category=Unit&Priority=High"
```

Filter syntax: [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests)

### Code Coverage

```bash
# Enable coverage collection
dotnet nuke --target UnitTests --collect-coverage true

# Coverage reports are in: results/coverage/* (Cobertura format)
```

**Important:** At least one test must execute and cover code when coverage is enabled.

## Git Commit Validation

### Conventional Commits

Projects using CloudTek.Git enforce [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Valid types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation changes
- `style` - Code style/formatting
- `refactor` - Code refactoring
- `perf` - Performance improvements
- `test` - Test changes
- `build` - Build system changes
- `ci` - CI/CD changes
- `chore` - Maintenance tasks
- `revert` - Revert previous commit

**Examples:**
```bash
git commit -m "feat: add user authentication"
git commit -m "fix(api): resolve null reference in UserController"
git commit -m "docs: update README with setup instructions"
```

### Validation

Commit messages are validated via git hooks (`.husky/`) using `dotnet-script`.

To check commits manually:
```bash
dotnet nuke --target CommitCheck
```

## Environment Variables

### NUKE Parameter Binding

All build parameters can be set via environment variables:

```bash
# Format options (use ONE, not both):
export TESTFILTER="Category=Unit"           # Option 1
export NUKE_TESTFILTER="Category=Unit"      # Option 2

# Other examples
export CONFIGURATION="Release"
export COLLECTCOVERAGE="true"
export PACKAGESFILTER="Microsoft"
```

**Important:** NUKE is case-insensitive and ignores non-alphanumeric characters. These all map to the same parameter:
- `TestFilter`
- `test-filter`
- `TESTFILTER`
- `NUKE_TEST_FILTER`

**Warning:** Do NOT set both `PARAMETERNAME` and `NUKE_PARAMETERNAME` - this causes binding failures.

### Test Environment Variables

**Critical:** SmartBuild propagates **ALL** environment variables to `dotnet test`.

Ensure integration test dependencies are configured in your shell profile:
- Bash/Zsh: `~/.bashrc`, `~/.zshrc`
- PowerShell: PowerShell profile
- Windows: System/User environment variables

## Project Setup (SmartBuild)

### Basic Setup (Default Versioning)

Create a NUKE build that inherits from SmartBuild:

```csharp
// build/Build.cs
using CloudTek.Build;
using CloudTek.Build.Versioning;

namespace _build;

public class Build : SmartBuild<VersioningStrategy.Default>
{
    public static int Main() => Execute<Build>(x => x.All);
}
```

### Advanced Setup (GitVersion)

For semantic versioning with GitVersion:

**Prerequisites:**
```bash
dotnet tool restore
dotnet nuke :add-package GitVersion.Tool --version 6.5.0
```

**Build definition:**
```csharp
// build/Build.cs
using CloudTek.Build;
using CloudTek.Build.Versioning;

namespace _build;

public class Build : SmartBuild<VersioningStrategy.GitVersion>
{
    [GitVersion(Framework = "net10.0", NoFetch = true)]
    public GitVersion GitVersion { get; set; } = default!;

    public static int Main() => Execute<Build>(x => x.All);
}
```

### Customization

Override properties to customize behavior:

```csharp
public class Build : SmartBuild<VersioningStrategy.Default>
{
    /// <summary>
    /// Filter packages in Beta/Outdated checks
    /// </summary>
    public override string PackagesFilter { get; init; } = "Nuke";

    /// <summary>
    /// Default test filter for all test targets
    /// </summary>
    protected override string TestFilter { get; init; } = "Flaky!=true";

    public static int Main() => Execute<Build>(x => x.Compile);
}
```

## Package Management

### Checking for Issues

```bash
# Check for pre-release packages
dotnet nuke --target BetaCheck

# Check for outdated packages
dotnet nuke --target OutdatedCheck

# Check for vulnerable packages
dotnet nuke --target VulnerabilityCheck

# All checks together
dotnet nuke --target RunChecks
```

### Filtering Package Checks

```bash
# Check only Microsoft packages
dotnet nuke --target OutdatedCheck --packages-filter "Microsoft"

# Check only Nuke packages
dotnet nuke --target BetaCheck --packages-filter "Nuke"
```

### Updating Packages

```bash
# Update all outdated packages
dotnet nuke --target UpdateOutdated
```

## Troubleshooting

### "Could not resolve {VariableName} since multiple values are provided"

**Cause:** Both `PARAMETERNAME` and `NUKE_PARAMETERNAME` environment variables are set.

**Solution:** Use only one format:
```bash
export TESTFILTER="Flaky!=true"    # Use this
unset NUKE_TESTFILTER               # Not both
```

### "At least one test must cover code"

**Cause:** Code coverage is enabled but no tests executed.

**Solution:**
- Verify test filter is not excluding all tests
- Ensure tests exist and are properly decorated with category attributes

### "HuskyInstall requires .git folder"

**Cause:** Running `CommitCheck` or `HuskyInstall` outside a git repository.

**Solution:** Skip the check if not in a git repo:
```bash
dotnet nuke --target RunChecks --skip CommitCheck
```

### Integration Tests Fail Locally

**Cause:** Missing environment variables or external dependencies (Docker, databases, etc.).

**Solution:**
1. Verify required services are running (check project-specific docs)
2. Check environment variables are set in your shell profile
3. Consider marking local-only tests:
   ```csharp
   [IntegrationTest]
   [SmartFact(On.All, Execute.InGithubActions)] // Skip locally
   public void CIOnlyIntegrationTest() { }
   ```

## Important Concepts

### Solution File as Source of Truth

The `.sln` file determines which projects are built and tested. Only projects referenced in the solution file are processed by SmartBuild.

### File System Agnostic

SmartBuild works with any directory structure. There are no requirements for specific folder layouts beyond having a valid `.sln` file.

### Configuration Defaults

- **Local development:** `Configuration=Debug`
- **CI/CD:** `Configuration=Release`

Override with `--configuration Release` if needed.

### Output Directories

- **Build artifacts:** `artifacts/packages/`, `artifacts/services/`, `artifacts/tests/`
- **Test results:** `results/tests/`
- **Code coverage:** `results/coverage/` (Cobertura format)

## Useful Commands

```bash
# List all available targets
dotnet nuke --help

# Show execution plan without running
dotnet nuke --target All --plan

# Visualize target dependencies
dotnet nuke --target All --graph

# Verbose logging
dotnet nuke --target Test --verbosity Verbose

# Run with specific parameters
dotnet nuke --target Test --configuration Release --collect-coverage true
```

## Claude Code Guidelines

When working with projects using CloudTek.Build:

1. **Always run `dotnet tool restore` first** - NUKE and other tools must be restored before use

2. **Respect test categories** - Tests must have `[UnitTest]`, `[IntegrationTest]`, etc. to be discovered

3. **Be cautious with `Format` target** - It modifies files. Use `FormatCheck` for validation only

4. **Environment variables are propagated to tests** - Remember this when debugging test failures

5. **Parameter naming is flexible** - `--test-filter`, `--TestFilter`, and `TESTFILTER` all work

6. **Prefer CLI `--skip` over `NUKE_SKIP`** - More explicit and visible

7. **Git hooks require `.git` folder** - `CommitCheck` will fail without it

8. **Beta packages must be pinned** - The `BetaCheck` is intentionally strict about pre-release packages

9. **Coverage requires test execution** - Empty test runs fail when `--collect-coverage` is enabled

10. **SmartBuild is inherited, not modified** - Projects extend it; the base class is in the NuGet package
