using System.Reflection;
using Nuke.Common.ProjectModel;

namespace CloudTek.Build.Packaging;

internal static class SolutionExtensions
{
  /// <summary>
  ///   Workaround for a bug detected in NUKE, where NUKE fails to properly list solution projects in a solution which is not
  ///   a top-level repository item.
  /// </summary>
  /// <param name="solution"></param>
  /// <returns>a collection of projects</returns>
  internal static IEnumerable<Project> GetPrimitiveProjects(this Solution solution)
  {
    var property = solution.GetType().GetProperty("PrimitiveProjects", BindingFlags.Instance | BindingFlags.NonPublic);
    var value = property?.GetValue(solution) as IEnumerable<PrimitiveProject>;

    return value?
      .Where(x => x.ToString() != "Nuke.Common.ProjectModel.SolutionFolder")
      .Where(x => x is Project)
      .Cast<Project>()
      .ToArray() ?? default!;
  }
}