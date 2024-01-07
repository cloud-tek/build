namespace CloudTek.Build.Utilities;

internal static class GenericExtensions
{
  internal static ISet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
  {
    return new HashSet<T>(source, comparer);
  }
}