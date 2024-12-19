using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Nuke.Common.IO;

namespace CloudTek.Build.Primitives;

// TODO: refactor

/// <summary>
/// Properties of a project
/// </summary>
/// <param name="IsPackable"></param>
/// <param name="PackAsTool"></param>
/// <param name="IsTestProject"></param>
/// <param name="OutputType"></param>
/// <param name="HasCodeCoveragePackage"></param>
public record ProjectProperties(
  bool? IsPackable,
  bool? PackAsTool,
  bool? IsTestProject,
  string? OutputType,
  bool? HasCodeCoveragePackage);

/// <summary>
/// Project type helper enum
/// </summary>
public enum ProjectType
{
  /// <summary>
  /// Project that is going to be emitted as a NuGet package artifact
  /// </summary>
  Package = 0,

  /// <summary>
  /// Project that is going to be emitted as a container image artifact
  /// </summary>
  Service,

  /// <summary>
  /// Project containing tests
  /// </summary>
  Test,

  /// <summary>
  /// Project that does not get emitted in any way
  /// </summary>
  Library,

  /// <summary>
  /// Project that is going to be emitted as a dotnet tool
  /// </summary>
  Tool
}

/// <summary>
/// A primitive describing a project in the repository
/// </summary>
public class Project
{
  internal AbsolutePath Path { get; init; } = default!;
  internal AbsolutePath WorkDir { get; init; } = default!;
  internal string Name { get; init; } = default!;
  internal ProjectProperties ProjectProperties { get; init; }

  internal ProjectType Type
  {
    get
    {
      if (ProjectProperties.IsTestProject == true)
      {
        return ProjectType.Test;
      }

      if (ProjectProperties.OutputType?.ToLowerInvariant() == "exe" && ProjectProperties.PackAsTool == true &&
          ProjectProperties.IsPackable == true)
      {
        return ProjectType.Package; //dotnet tool
      }

      if (ProjectProperties.OutputType?.ToLowerInvariant() == "exe")
      {
        return ProjectType.Service; //otherwise if it is an exe we treat it as service
      }

      if (ProjectProperties.IsPackable == true)
      {
        return ProjectType.Package; //otherwise if it is packable - pack it
      }

      return ProjectType.Library; //if not - it is just library
    }
  }

  internal Project(AbsolutePath path)
  {
    var (assemblyName, projectProperties) = GetAssemblyInformation(path);

    Path = path;
    WorkDir = path.Parent!;
    ProjectProperties = projectProperties;
    Name = assemblyName;
  }

  private static (string assemblyName, ProjectProperties projectProperties) GetAssemblyInformation(AbsolutePath path)
  {
    var msProject = ProjectCollection.GlobalProjectCollection.LoadedProjects.SingleOrDefault(p => p.FullPath == path)
                    ?? Microsoft.Build.Evaluation.Project.FromFile(
                      path,
                      new ProjectOptions()
                      {
                        LoadSettings = ProjectLoadSettings.IgnoreMissingImports |
                                       ProjectLoadSettings.IgnoreInvalidImports
                      });
    var assemblyNameElement = msProject.GetPropertyValue("AssemblyName");
    var assemblyName = !string.IsNullOrEmpty(assemblyNameElement) ? assemblyNameElement : path.NameWithoutExtension;

    var isPackable = TryGetBool(msProject.GetPropertyValue("IsPackable"));
    var outputType = msProject.GetPropertyValue("OutputType");
    var isTestProject = msProject.Items.Any(
      p => p.ItemType == "PackageReference" && p.EvaluatedInclude == "Microsoft.NET.Test.Sdk");
    var hasCodeCoveragePackage =
      msProject.Items.Any(p => p.ItemType == "PackageReference" && p.EvaluatedInclude == "coverlet.collector");
    var packAsTool = TryGetBool(msProject.GetPropertyValue("PackAsTool"));
    var projectProperties = new ProjectProperties(
      isPackable,
      packAsTool,
      isTestProject,
      outputType,
      hasCodeCoveragePackage);

    return (assemblyName, projectProperties);
  }

  private static bool? TryGetBool(string value)
  {
    bool? parsed = bool.TryParse(value, out var p) ? p : null;
    return parsed;
  }
}