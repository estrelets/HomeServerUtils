using System.Diagnostics;
using System.Globalization;

namespace Converter;

public class GenericConverter
{
    private readonly string _binPath;
    private readonly string _argPattern;

    public GenericConverter(string binPath, string argPattern)
    {
        _binPath = binPath;
        _argPattern = argPattern;
    }

    public async Task Convert(string input, string output, CancellationToken ct)
    {
        try
        {
            var sw = Stopwatch.StartNew();

            await ConvertInternal(input, output, ct);
            FixCreateTime(input, output);
            
            sw.Stop();
            Console.WriteLine($"Convert success {input} -> {output} in {sw.Elapsed}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Convert error {input} -> {output}: {ex}");
        }
    }

    private async Task ConvertInternal(string from, string to, CancellationToken ct)
    {
        using var temp = new TempFile(Path.GetExtension(to));
        
        var args = string.Format(_argPattern, from, temp.Path);
        var process = Process.Start(new ProcessStartInfo(_binPath, args));
        await process.WaitForExitAsync(ct);
        
        File.Move(temp.Path, to, true);
    }

    private void FixCreateTime(string input, string output)
    {
        var createDate = ExtractCreateTime(input);

        File.SetCreationTime(output, createDate);
        File.SetCreationTimeUtc(output, createDate);
        File.SetLastWriteTime(output, createDate);
        File.SetLastWriteTimeUtc(output, createDate);
    }
    
    static DateTime ExtractCreateTime(string filePath)
    {
        //*input 2020-08-23 16-22-36
        
        var file = new FileInfo(filePath);
        var name = Path.GetFileNameWithoutExtension(file.Name);

        if (DateTime.TryParseExact(name, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return dateTime;
        }

        return file.CreationTimeUtc < file.LastWriteTime
            ? file.CreationTimeUtc
            : file.LastWriteTime;
    }
}