using Nuke.Common.IO;

namespace CloudTek.Build.Tests;

internal static class TestHelpers
{
  internal static AbsolutePath GetRepositoryRoot(string currentPath)
  {
    var directory = new DirectoryInfo(currentPath);
    while (directory != null && directory.GetDirectories(".git").Length == 0)
    {
      directory = directory.Parent;
    }

    return AbsolutePath.Create(directory?.FullName!);
  }
}