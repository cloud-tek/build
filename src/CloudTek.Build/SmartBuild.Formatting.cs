using System.Globalization;
using System.Text;
using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  ///   dotnet nuke --target Restore --skip-format-check
  /// </summary>
  [Parameter]
  public bool SkipFormatCheck { get; set; }

  /// <summary>
  ///   dotnet nuke --target Restore --skip-format-analyzers-check
  /// </summary>
  [Parameter]
  public bool SkipFormatAnalyzersCheck { get; set; }

  /// <summary>
  /// dotnet nuke --target Format
  /// Executes dotnet format against the solution
  /// </summary>
  protected virtual Target Format => _ => _
    .DependsOn(Restore)
    .Executes(() =>
    {
      FormatInternal(false);
      FormatAnalyzersInternal(false);
    });

  /// <summary>
  ///  dotnet nuke --target FormatCheck --skip-format-check
  /// </summary>
  protected virtual Target FormatCheck => _ => _
    .OnlyWhenDynamic(() => !SkipFormatCheck)
    .DependsOn(Restore)
    .Executes(() => { FormatInternal(true); });

  /// <summary>
  /// dotnet nuke --target FormatAnalyzersCheck --skip-format-analyzers-check
  /// </summary>
  protected virtual Target FormatAnalyzersCheck => _ => _
    .OnlyWhenDynamic(() => !SkipFormatAnalyzersCheck)
    .DependsOn(Restore)
    .Executes(() => { FormatAnalyzersInternal(true); });

  private void FormatAnalyzersInternal(bool noChanges)
  {
    var sb = new StringBuilder("format analyzers ");
    if (noChanges)
      sb.Append("--verify-no-changes ");

    sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");
    sb.Append("--no-restore");

    DotNet(sb.ToString().TrimEnd(), Solution.Directory);
  }

  private void FormatInternal(bool noChanges)
  {
    var sb = new StringBuilder("format ");
    if (noChanges)
      sb.Append("--verify-no-changes ");

    sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");
    sb.Append("--no-restore");

    DotNet(sb.ToString().TrimEnd(), Solution.Directory);
  }
}