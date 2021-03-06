# cloud-tek/build

Contains the shared build system based on NUKE.
The `CloudTek.Build` package is meant to be a reusable build system for a variety of projects which are to share behavior in a large multi-repo scenario.

## Usage

Create a NUKE build in your project. Add `CloudTek.Build` package.

Define a `SmartBuild` in one of the following ways:

### SmartBuild with a default versioning strategy

```csharp
[CheckBuildProjectConfigurations]
public class Build : SmartBuild<VersioningStrategy.Default>
{
    public static int Main () => Execute<Build>(x => x.Compile);
        
    public Build() : base(Repository)
    { }

    static new readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Project = "CloudTek.Build" }
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
[CheckBuildProjectConfigurations]
public class Build : SmartGitVersionBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);
        
    public Build() : base(Repository)
    { }

    static new readonly Repository Repository = new ()
    {
        Artifacts = new []
        {
            new Artifact() { Type = ArtifactType.Package, Project = "CloudTek.Build" }
        }
    };
}
```