namespace Converter;

public class TempFile : IDisposable
{
    public string Path { get; init; }

    public TempFile(string extension)
    {
        var path = System.IO.Path.GetTempFileName();
        Path = System.IO.Path.ChangeExtension(path, "mp4");
    }
    
    public void Dispose()
    {
        try
        {
            File.Delete(Path);
        }
        catch
        {
            //ignore
        }
    }
}