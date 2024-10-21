using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

namespace CloudTek.Build.Primitives;

/// <summary>
/// A primitive describing a project in the repository
/// </summary>
public class Project
{
  internal Project(AbsolutePath path, ArtifactType type)
  {
    var assemblyInfo = GetAssemblyInformation(path);

    Path = path;
    WorkDir = path.Parent!;
    Type = type;
    Name = assemblyInfo.AssemblyName;
    IsPackable = assemblyInfo.IsPackable;
  }

  /// <summary>
  /// The path to the artifact's project file
  /// </summary>
  public AbsolutePath Path { get; init; } = default!;

  /// <summary>
  /// The directory where the project is located
  /// </summary>
  public AbsolutePath WorkDir { get; init; } = default!;

  /// <summary>
  /// Assembly name of the artifact
  /// </summary>
  public string Name { get; init; } = default!;

  /// <summary>
  /// Flag indicating whether the artifact is Packable
  /// </summary>
  public bool IsPackable { get; init; }

  /// <summary>
  /// Type of the artifact
  /// </summary>
  public ArtifactType Type { get; init; }

  private static (string AssemblyName, bool IsPackable) GetAssemblyInformation(AbsolutePath path)
  {
    var msProject = ProjectCollection.GlobalProjectCollection.LoadedProjects.SingleOrDefault(p => p.FullPath == path)
                    ?? Microsoft.Build.Evaluation.Project.FromFile(path, new ProjectOptions
                    {
                      LoadSettings = ProjectLoadSettings.IgnoreMissingImports | ProjectLoadSettings.IgnoreInvalidImports
                    });
    var assemblyNameElement = msProject.GetPropertyValue("AssemblyName");
    var assemblyName = !string.IsNullOrEmpty(assemblyNameElement) ? assemblyNameElement : path.NameWithoutExtension;

    var isPackableValue = msProject.GetPropertyValue("IsPackable");
    var isPackable = !isPackableValue.IsNullOrEmpty() && bool.Parse(isPackableValue);

    return (assemblyName, isPackable);
  }
}