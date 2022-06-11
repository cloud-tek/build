namespace CloudTek.Build.Versioning
{
    public interface IAssemblyVersionHandler
    {
        string Handle(string path);
    }
}