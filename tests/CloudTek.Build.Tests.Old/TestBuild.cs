using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CloudTek.Build;
using CloudTek.Build.Packaging;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

// ReSharper disable once CheckNamespace
namespace CloudTek.Build.Tests;

public class TestBuild : SmartBuild<PackageManager.NuGet, VersioningStrategy.Default>
{
  public static new readonly Repository Repository = new()
  {
    Artifacts = new Dictionary<string, ArtifactType>
      {
        { "CloudTek.Build", ArtifactType.Package },
        { "CloudTek.Testing", ArtifactType.Package }
      }
      .Select(x => new Artifact { Type = x.Value, Path = RootDirectory / "src" / x.Key / "*.*sproj" })
      .ToArray()
  };

  public TestBuild(Func<Solution> solutionProvider)
    : base(Repository, solutionProvider)
  {
  }

#pragma warning disable IDE0051
  public static int Exec<T>(params Expression<Func<T, Target>>[] defaultTargetExpressions)
    where T : NukeBuild, new()
  {
    return Execute<T>(defaultTargetExpressions);
  }
#pragma warning restore IDE0051
}