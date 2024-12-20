using System.Globalization;
using System.Text;
using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    /// <summary>
    /// dotnet nuke --target Format
    /// (!) Warning, causes side effects and modifies code.
    /// </summary>
    protected virtual Target Format => _ => _
      .DependsOn(Restore)
      .Executes(
        () =>
        {
          FormatInternal(false);
          FormatAnalyzersInternal(false);
        });

    /// <summary>
    /// dotnet nuke --target FormatCheck
    /// Runs dotnet format --no-verify
    /// </summary>
    protected virtual Target FormatCheck => _ => _
      .DependsOn(Restore)
      .Description("Run format check using: dotnet format --no-verify")
      .Executes(
        () =>
        {
          FormatInternal(true);
        });

    /// <summary>
    /// dotnet nuke --target FormatAnalyzersCheck
    /// Runs format check using: dotnet format analyzers --no-verify
    /// </summary>
    protected virtual Target FormatAnalyzersCheck => _ => _
      .Description("Run format check using: dotnet format analyzers --no-verify")
      .DependsOn(Restore)
      .Executes(
        () =>
        {
          FormatAnalyzersInternal(true);
        });

    private void FormatAnalyzersInternal(bool noChanges)
    {
      var sb = new StringBuilder("format analyzers ");
      if (noChanges)
        sb.Append("--verify-no-changes ");

      sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");

      DotNet(sb.ToString().TrimEnd(), Solution.Directory);
    }

    private void FormatInternal(bool noChanges)
    {
      var sb = new StringBuilder("format ");
      if (noChanges)
        sb.Append("--verify-no-changes ");

      sb.Append(CultureInfo.InvariantCulture, $"./{Solution.FileName}");

      DotNet(sb.ToString().TrimEnd(), Solution.Directory);
    }
  }
}