using System.Text.RegularExpressions;

namespace CloudTek.Git;

/// <summary>
/// Constants class containing regular expressions for conventional commits.
/// </summary>
public static partial class Constants
{
  private const string ConventionalCommitsMessageRegexExpression =
    @"^(?:build|feat|ci|chore|docs|fix|perf|pr|refactor|revert|style|test)(?:\(.+\))*(?::).{4,}(?:#\d+)*(?<![\.\s])$";

  /// <summary>
  /// Regular expression for matching conventional commit messages.
  /// </summary>
  [GeneratedRegex(ConventionalCommitsMessageRegexExpression, RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
  public static partial Regex ConventionalCommitsMessageRegex();
}