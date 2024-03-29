using System.Globalization;
using System.Text;
using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  /// dotnet nuke --target Format
  /// Executes dotnet format against the solution
  /// </summary>
  protected internal virtual Target Format => _ => _
    .CheckIfSkipped(nameof(Format), this)
    .DependsOn(Restore)
    .Executes(() =>
    {
      FormatInternal(false);
      FormatAnalyzersInternal(false);
    });

  /// <summary>
  ///  dotnet nuke --target FormatCheck --skip-format-check
  /// </summary>
  protected internal virtual Target FormatCheck => _ => _
    .CheckIfSkipped(nameof(FormatCheck), this)
    .DependsOn(Restore)
    .Executes(() => { FormatInternal(true); });

  /// <summary>
  /// dotnet nuke --target FormatAnalyzersCheck --skip-format-analyzers-check
  /// </summary>
  protected internal virtual Target FormatAnalyzersCheck => _ => _
    .CheckIfSkipped(nameof(FormatAnalyzersCheck), this)
    .DependsOn(Restore)
    .Executes(() => { FormatAnalyzersInternal(true); });

  private void FormatAnalyzersInternal(bool noChanges)
  {
    var sb = new StringBuilder("format analyzers ");
    if (noChanges)
      sb.Append("--verify-no-changes ");

    sb.Append("--no-restore ");
    sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");

    DotNet(sb.ToString().TrimEnd(), Solution.Directory);
  }

  private void FormatInternal(bool noChanges)
  {
    var sb = new StringBuilder("format ");
    if (noChanges)
      sb.Append("--verify-no-changes ");

    sb.Append("--no-restore ");
    sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");

    DotNet(sb.ToString().TrimEnd(), Solution.Directory);
  }
}