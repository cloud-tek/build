using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;

// ReSharper disable once CheckNamespace
namespace _build;

public class Build : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
  new static readonly Repository Repository = new()
  {
    Artifacts = new Dictionary<string, ArtifactType>
      {
        { "CloudTek.Build", ArtifactType.Package },
        { "CloudTek.Testing", ArtifactType.Package }
      }
      .Select(x => new Artifact { Type = x.Value, Path = RootDirectory / "src" / x.Key / "*.*sproj" })
      .ToArray()
  };

  public override Regex PackageChecksRegex { get; init; } = new Regex("^Nuke", RegexOptions.Compiled);

  public Build() : base(Repository)
  {
  }

  public static int Main() => Execute<Build>(x => x.Compile);
}