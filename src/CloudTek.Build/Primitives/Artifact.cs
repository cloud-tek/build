using Nuke.Common.IO;

namespace CloudTek.Build.Primitives;

/// <summary>
/// An enumeration describing possible types of artifacts emitted by the SmartBuild
/// </summary>
public enum ArtifactType
{
  /// <summary>
  ///   Artifact that is going to be emitted as a NuGet package
  /// </summary>
  Package = 0,

  /// <summary>
  ///   Artifact that is going to be emitted as a container image
  /// </summary>
  Service,

  /// <summary>
  ///   Artifact that is going to be emitted as a container image
  /// </summary>
  Demo,

  /// <summary>
  ///   Artifact that is going to be emitted as a dotnet tool
  /// </summary>
  Tool
}

/// <summary>
/// An abstraction representing one or more artifacts matching a Path glob pattern
/// </summary>
public sealed class Artifact : RepositoryContent
{
  private AbsolutePath _path = default!;
  private IEnumerable<Project> _projects = default!;

  /// <summary>
  /// Projects making up the artifact
  /// </summary>
  public IEnumerable<Project> Projects
  {
    get
    {
      if (_projects == null)
        throw new InvalidOperationException(
          "Artifact's projects have not been computed yet. Call .Initialize() first");

      return _projects;
    }
    private set => _projects = value;
  }

  /// <summary>
  /// Path to the artifact's project file(s)
  /// </summary>
  public AbsolutePath Path
  {
    get
    {
      if (_path == null)
        throw new SmartBuildException(SmartBuildError.NoArtifactPath);

      return _path;
    }
    set => _path = value;
  }

  /// <summary>
  /// Type of the artifact
  /// </summary>
  public ArtifactType Type { get; set; }

  /// <summary>
  /// Initializes the artifact. Determines the projects that are part of the artifact.
  /// </summary>
  public void Initialize()
  {
    Projects = Path.GlobFiles().Select(path => new Project(path, Type));
  }
}