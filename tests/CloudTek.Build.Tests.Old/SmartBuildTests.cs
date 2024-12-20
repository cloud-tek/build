#pragma warning disable CA1861
using System.Collections.ObjectModel;
using CloudTek.Testing;
using FluentAssertions;
using Xunit;

namespace CloudTek.Build.Tests;

public class SmartBuildTests : TestBuildFixture
{
  [SmartTheory(Execute.Always)]
  [UnitTest]
  [InlineData(
    nameof(SmartBuild.PackagesOutdatedCheck),
    nameof(SmartBuild.PackagesOutdatedCheck),
    true,
    nameof(SmartBuild.Compile))]
  [InlineData(
    nameof(SmartBuild.PackagesOutdatedCheck),
    nameof(SmartBuild.PackagesOutdatedCheck),
    false,
    nameof(SmartBuild.PackagesOutdatedCheck))]
  [InlineData(
    nameof(SmartBuild.PackagesBetaCheck),
    $"{nameof(SmartBuild.PackagesOutdatedCheck)} {nameof(SmartBuild.PackagesBetaCheck)}",
    true,
    nameof(SmartBuild.PackagesOutdatedCheck))]
  [InlineData(
    nameof(SmartBuild.PackagesBetaCheck),
    $"{nameof(SmartBuild.PackagesOutdatedCheck)} {nameof(SmartBuild.PackagesBetaCheck)}",
    false,
    nameof(SmartBuild.PackagesBetaCheck))]
  [InlineData(
    nameof(SmartBuild.PackagesBetaCheck),
    "",
    false,
    nameof(SmartBuild.PackagesOutdatedCheck))]
  public void GivenCurrentTarget_WhenCurrentTargetToBeSkippedUsingNukeSkip_ThenTargetSkipped(
    string currentTarget,
    string targetsToSkip,
    bool shouldSkip,
    params string[] invokedTargets)
  {
    var env = GetEnvironment(TargetDefinitionExtensions.NukeSkipList, targetsToSkip);
    TargetDefinitionExtensions.ShouldSkipTarget(currentTarget, invokedTargets, env).Should().Be(shouldSkip);
  }

  [SmartTheory(Execute.Always)]
  [UnitTest]
  [InlineData(
    nameof(SmartBuild.PackagesOutdatedCheck),
    nameof(SmartBuild.PackagesOutdatedCheck),
    true,
    nameof(SmartBuild.Compile))]
  [InlineData(
    nameof(SmartBuild.PackagesOutdatedCheck),
    nameof(SmartBuild.PackagesOutdatedCheck),
    false,
    nameof(SmartBuild.PackagesOutdatedCheck))]
  [InlineData(
    nameof(SmartBuild.PackagesOutdatedCheck),
    nameof(SmartBuild.PackagesBetaCheck),
    false,
    nameof(SmartBuild.Compile))]
  public void GivenCurrentTarget_WhenCurrentTargetToBeSkippedUsingNukeSkipPrefix_ThenTargetSkipped(
    string currentTarget,
    string targetToSkip,
    bool shouldSkip,
    params string[] invokedTargets)
  {
    var env = GetEnvironment($"{TargetDefinitionExtensions.NukeSkipPrefix}{targetToSkip}", "true");
    TargetDefinitionExtensions.ShouldSkipTarget(currentTarget, invokedTargets, env).Should().Be(shouldSkip);
  }

  [SmartFact(Execute.Always)]
  [UnitTest]
  public void GivenCurrentTarget_WhenCurrentTargetNotToBeSkippedUsingNukeSkip_ThenTargetNotSkipped()
  {
    var env = GetEnvironment($"{TargetDefinitionExtensions.NukeSkipPrefix}{nameof(SmartBuild.PackagesOutdatedCheck)}", "false");
    TargetDefinitionExtensions.ShouldSkipTarget(nameof(SmartBuild.PackagesOutdatedCheck), new string[] { nameof(SmartBuild.Compile) }, env).Should().BeFalse();
  }

  private static ReadOnlyDictionary<TKey, TValue> GetEnvironment<TKey, TValue>(TKey key, TValue value)
    where TKey : notnull
  {
    return new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>() { { key, value } });
  }
}
#pragma warning restore CA1861