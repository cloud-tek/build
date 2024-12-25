using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

namespace CloudTek.Build.Internals;

internal static class EmbeddedResourceExtensions
{
  /// <summary>
  /// Retrieves the names of all embedded resources matchin a prefix
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="prefix"></param>
  /// <returns></returns>
  internal static IEnumerable<string> GetEmbeddedResources(this Assembly assembly, string prefix)
  {
    var name = assembly.GetName().Name;

    prefix = prefix.StartsWith(name!) ? prefix : $"{name}.{prefix}";

    return assembly.GetManifestResourceNames().Where(p => p.StartsWith(prefix));
  }

  /// <summary>
  /// Copies all embedded resources which match the prefix, to a target path
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="path"></param>
  /// <param name="prefix"></param>
  /// <returns></returns>
  internal static async Task<IEnumerable<AbsolutePath>> CopyResources(
    this Assembly assembly,
    AbsolutePath path,
    string prefix)
  {
    var resources = assembly.GetEmbeddedResources(prefix);
    var result = new List<AbsolutePath>();
    foreach (var resource in resources)
    {
      result.Add(await assembly.CopyResource(
        path: path,
        resource: resource));
    }

    return result.ToArray();
  }

  /// <summary>
  /// Copies embedded resource into a file directly from the ManifestResourceStream
  /// </summary>
  /// <param name="assembly"></param>
  /// <param name="path"></param>
  /// <param name="resource"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentNullException"></exception>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="InvalidOperationException"></exception>
  internal static async Task<AbsolutePath> CopyResource(this Assembly assembly, AbsolutePath path, string resource)
  {
    _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
    _ = path ?? throw new ArgumentNullException(nameof(path));
    _ = !string.IsNullOrEmpty(resource) ? resource : throw new ArgumentException(nameof(resource));

    var asm = assembly.GetName().Name;
    var fullResourceName = resource.StartsWith(asm!) ? resource : $"{asm}.{resource}";

    await using var stream = assembly.GetManifestResourceStream(fullResourceName);
    if (stream == null)
    {
      throw new InvalidOperationException($"Unable to copy {resource}. Embedded resource not found");
    }

    var fileName = resource
      .Replace("..templates", string.Empty)
      .Replace(asm!, string.Empty)
      .ConvertFromEmbeddedResourcePath();

    var target = path / fileName;

    if (!Directory.Exists(target.Parent))
    {
      Directory.CreateDirectory(target.Parent);
    }

    await using var file = new FileStream(target, FileMode.Create);
    await stream.CopyToAsync(file);

    return target;
  }

  // internal static async Task CopyEmbeddedResourceAsync(this Assembly assembly, string resource)
  // {
  //   _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
  //   if (resource.IsNullOrEmpty())
  //   {
  //     throw new ArgumentNullException(nameof(resource));
  //   }
  //
  //   await using var stream = assembly.GetManifestResourceStream(resource);
  //   if (stream == null)
  //   {
  //     return;
  //   }
  //
  //   using var reader = new StreamReader(stream);
  //   var strTxt = await reader.ReadToEndAsync();
  //   var targetFileName = ConvertEmbeddedResourcePath((resource.Replace(assembly.GetName().Name + $".{stream}.", "")));
  //   targetFileName = targetFileName.ReplaceTokens(replaceTags);
  //   var targetFile = folderPath / targetFileName;
  //   var directory = Path.GetDirectoryName(targetFile);
  //
  //   if (directory != null)
  //   {
  //     Directory.CreateDirectory(directory);
  //   }
  //
  //   await File.WriteAllTextAsync(targetFile, strTxt.ReplaceTokens(replaceTags));
  // }

  internal static string ConvertFromEmbeddedResourcePath(this string path)
  {
    var lastDotIndex = path.LastIndexOf(".", StringComparison.Ordinal);
    var extensionLength = path.Length - lastDotIndex - 1;

    if (lastDotIndex != -1)
    {
      // replace dots with slashes except when this would create two slashes in a row or the dot is escaped with a dollar sign
      path = path
        .ReplaceRegex(@"([^\\$])\.", x => $"{x.Groups[1]}/");

      var sb = new StringBuilder(path);

      if (extensionLength <= 5)
      {
        sb[lastDotIndex] = '.';
      }

      path = sb.ToString()
        .Replace("$.", ".");
    }

    return path.Replace("__", "_");
  }
  internal static string ReplaceTokens(this string input, Dictionary<string, string>? tokens)
  {
    _ = input ?? throw new ArgumentNullException(nameof(input));

    if (tokens == null)
    {
      return input;
    }

    var sb = new StringBuilder(input);

    foreach (var (key, value) in tokens)
    {
      sb.Replace(key, value);
    }

    return sb.ToString();
  }
}