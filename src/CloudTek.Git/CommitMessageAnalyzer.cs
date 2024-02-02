using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace CloudTek.Git;

/// <summary>
/// Analyzes commit messages and checks their validity against conventional commits convention and some CI exceptions
/// </summary>
public static class CommitMessageAnalyzer
{
  private static readonly string[] ExceptionMessages = new[] { "merge", "created branch", "apply suggestions" };

  /// <summary>
  /// Analyzes last commit message based on the provided filename
  /// </summary>
  /// <param name="fileName"></param>
  /// <returns>0 if everything is ok, 1 if commit message is invalid, 2 if commit message is empty</returns>
  /// <exception cref="InvalidOperationException">Thrown if an unhandled result is returned</exception>
  public static int AnalyzeCommitsFromFile(string fileName)
  {
    var msg = File.ReadAllLines(fileName);

    var result = Analyze(msg[0]);

    return HandleResult(result);
  }

  /// <summary>
  /// Analyzes last commit message based on the provided filename
  /// </summary>
  /// <param name="args"></param>
  /// <returns>0 if everything is ok, 1 if commit message is invalid, 2 if commit message is empty</returns>
  /// <exception cref="InvalidOperationException">Thrown if an unhandled result is returned</exception>
  public static int AnalyzeCommitsFromLog(params string[] args)
  {
    var result = RunGitLog(args);

    return HandleResult(result);
  }

  private static CommitMessageAnalysisResult RunGitLog(params string[] args)
  {
    var procStartInfo = new ProcessStartInfo("git", $"log {args[0]} --format=%s")
    {
      RedirectStandardError = true,
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    var sb = new StringBuilder();
    var proc = new Process();
    var result = CommitMessageAnalysisResult.Ok;
    proc.StartInfo = procStartInfo;

    proc.OutputDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        result = Analyze(e.Data);

        if (result == CommitMessageAnalysisResult.Invalid)
          sb.AppendLine(e.Data);
      }
    };

    proc.ErrorDataReceived += (sender, e) =>
    {
      if (string.IsNullOrWhiteSpace(e.Data))
        return;
      sb.AppendLine(e.Data);
    };

    proc.Start();
    proc.BeginOutputReadLine();
    proc.BeginErrorReadLine();
    proc.WaitForExit();

    return result;
  }

  private static int HandleResult(CommitMessageAnalysisResult result)
  {
    switch (result)
    {
      case CommitMessageAnalysisResult.Ok:
        return 0;
      case CommitMessageAnalysisResult.Invalid:
        return 1;
      case CommitMessageAnalysisResult.Empty:
        return 2;
      default:
        throw new InvalidOperationException($"Unhandled result type: {result.ToString()}");
    }
  }

  /// <summary>
  /// Checks if the commit message is valid
  /// </summary>
  /// <param name="message"></param>
  /// <returns>An enum containing the analysis result</returns>
  public static CommitMessageAnalysisResult Analyze(string message)
  {
    if (string.IsNullOrEmpty(message))
    {
      return CommitMessageAnalysisResult.Empty;
    }

    if (ExceptionMessages.Any(
          msg => message
            .ToLower(CultureInfo.InvariantCulture)
            .StartsWith(msg, StringComparison.OrdinalIgnoreCase)))
    {
      return CommitMessageAnalysisResult.Ok;
    }

    if (Constants.ConventionalCommitsMessageRegex().IsMatch(message))
    {
      return CommitMessageAnalysisResult.Ok;
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Invalid commit messages:");
    Console.ResetColor();
    Console.WriteLine(message);
    Console.WriteLine("e.g: 'feat(scope): subject' or 'feat: subject'");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("more info: https://www.conventionalcommits.org/en/v1.0.0/");

    return CommitMessageAnalysisResult.Invalid;
  }
}