using CloudTek.Git;
using CloudTek.Testing;
using FluentAssertions;
using Xunit;

namespace CloudTek.Git.Tests;

public class CommitMessageAnalyzersTests
{
  // feat|ci|chore|docs|fix|perf|pr|refactor|revert|style|test
  [UnitTest]
  [SmartTheory(Execute.Always)]
  [InlineData("", CommitMessageAnalysisResult.Empty)]
  [InlineData("chore: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("feat: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("docs: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("fix: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("ci: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("perf: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("pr: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("refactor: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("revert: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("style: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("test: 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("chore(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("feat(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("docs(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("fix(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("ci(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("perf(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("pr(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("refactor(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("revert(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("style(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("test(123): 123", CommitMessageAnalysisResult.Ok)]
  [InlineData("I fixed something", CommitMessageAnalysisResult.Invalid)]
  [InlineData("New feature added", CommitMessageAnalysisResult.Invalid)]
  public void GivenAnalyze_WhenStandardMessage_ThenValidResult(string message, CommitMessageAnalysisResult expected)
  {
    var result = CommitMessageAnalyzer.Analyze(message);

    result.Should().Be(expected);
  }
}