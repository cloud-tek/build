namespace CloudTek.Build.Primitives;

public abstract class RepositoryContent
{
    /// <summary>
    /// Optional, specifies the source directory root for a set of artifacts & tests
    /// </summary>
    public string Module { get; set; } = default!;
}