using Nuke.Common.Git;
using Nuke.Common.Utilities;

namespace CloudTek.Build.Versioning;

internal static class GitRepositoryExtensions
{
  internal static bool IsOnBugfixBranch(this GitRepository repository)
  {
    return (repository.Branch?.StartsWithOrdinalIgnoreCase("bug/") ?? false) ||
           (repository.Branch?.StartsWithOrdinalIgnoreCase("fix/") ?? false) ||
           (repository.Branch?.StartsWithOrdinalIgnoreCase("hotfix/") ?? false) ||
           (repository.Branch?.StartsWithOrdinalIgnoreCase("bugfix/") ?? false);
  }
}