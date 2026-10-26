using System.IO;
using System.Linq;
using System.Text.Json;
using Nuke.Common.IO;

namespace CloudTek.Build.Internals;

/// <summary>
/// Read-only access to dotnet local tool manifests (dotnet-tools.json).
/// </summary>
internal static class ToolManifest
{
  private static readonly JsonDocumentOptions Options = new()
  {
    AllowTrailingCommas = true,
    CommentHandling = JsonCommentHandling.Skip,
  };

  /// <summary>
  /// Determines whether the manifest JSON lists the given tool package.
  /// </summary>
  /// <param name="manifestJson">The manifest content.</param>
  /// <param name="packageId">The tool package id (case-insensitive).</param>
  /// <returns>True when the tools section contains the package.</returns>
  internal static bool ListsTool(string manifestJson, string packageId)
  {
    using var document = JsonDocument.Parse(manifestJson, Options);
    var root = document.RootElement;

    if (root.ValueKind != JsonValueKind.Object
      || !root.TryGetProperty("tools", out var tools)
      || tools.ValueKind != JsonValueKind.Object)
    {
      return false;
    }

    return tools.EnumerateObject()
      .Any(p => string.Equals(p.Name, packageId, StringComparison.OrdinalIgnoreCase));
  }

  /// <summary>
  /// Determines whether the manifest JSON is marked as a root manifest.
  /// </summary>
  /// <param name="manifestJson">The manifest content.</param>
  /// <returns>True only when isRoot is the JSON value true.</returns>
  internal static bool IsRoot(string manifestJson)
  {
    using var document = JsonDocument.Parse(manifestJson, Options);
    var root = document.RootElement;

    return root.ValueKind == JsonValueKind.Object
      && root.TryGetProperty("isRoot", out var isRoot)
      && isRoot.ValueKind == JsonValueKind.True;
  }

  /// <summary>
  /// Walks from the start directory up through its parents, as the SDK does,
  /// and determines whether a manifest lists the given tool.
  /// Malformed manifests and IO failures are not caught.
  /// </summary>
  /// <param name="startDirectory">The directory to start from.</param>
  /// <param name="packageId">The tool package id.</param>
  /// <returns>True when a manifest in scope lists the package.</returns>
  internal static bool ContainsTool(AbsolutePath startDirectory, string packageId)
  {
    var directory = startDirectory.ToString();

    while (!string.IsNullOrEmpty(directory))
    {
      foreach (var file in ManifestFiles(directory))
      {
        var json = File.ReadAllText(file);

        if (ListsTool(json, packageId))
        {
          return true;
        }

        if (IsRoot(json))
        {
          return false;
        }
      }

      directory = Path.GetDirectoryName(directory);
    }

    return false;
  }

  private static IEnumerable<string> ManifestFiles(string directory)
  {
    return new[]
    {
      Path.Combine(directory, ".config", "dotnet-tools.json"),
      Path.Combine(directory, "dotnet-tools.json"),
    }.Where(File.Exists);
  }
}