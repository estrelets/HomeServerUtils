using System.Diagnostics;
using System.Globalization;

var (loaded, directory, command) = Init();
if (!loaded) return -1;

foreach (var file in EnumerateFiles(directory))
{
    await Convert(file, command);
}

return 0;



static IEnumerable<FileInfo> EnumerateFiles(string directoryPath)
{
    var directory = new DirectoryInfo(directoryPath);
    return     directory.EnumerateFiles("*.mov", SearchOption.AllDirectories)
        .Union(directory.EnumerateFiles("*.MOV", SearchOption.AllDirectories));    
}

static async Task Convert(FileInfo file, string command)
{
    Log($"Start convert: {file.Name}");
    
    try
    {
        var inputPath = file.FullName;
        var outputPath = Path.ChangeExtension(file.FullName, "mp4");
        
        var args = String.Format(command, inputPath, outputPath);
        var process = Process.Start(new ProcessStartInfo("ffmpeg", args));
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Log($"Error while converting {file.Name}");
            return;
        }

        var createDate = ExtractCreateTime(file);
        
        File.SetCreationTime(outputPath, createDate);
        File.SetCreationTimeUtc(outputPath, createDate);
        File.SetLastWriteTime(outputPath, createDate);
        File.SetLastWriteTimeUtc(outputPath, createDate);
        
        file.Delete();
    }
    catch (Exception ex)
    {
        Log($"Error while converting {file.Name}: {ex}");
    }
}

static DateTime ExtractCreateTime(FileInfo file)
{
    //*input 2020-08-23 16-22-36
    var name = Path.GetFileNameWithoutExtension(file.Name);
    
    if (DateTime.TryParseExact(name, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
    {
        return dateTime;
    }

    return file.CreationTimeUtc < file.LastWriteTime
        ? file.CreationTimeUtc
        : file.LastWriteTime;
}

static (bool, string, string) Init()
{
    const string directoryVarName = "DIRECTORY";
    const string ffmpegCommandVarName = "FFMPEG_COMMAND";
    const string defaultFfmpegCommand = "-y -noautorotate -i \"{0}\" -c:v libx264 -codec:a libmp3lame -vf format=yuv420p -movflags +faststart \"{1}\"";
    
    var directoryPath = Environment.GetEnvironmentVariable(directoryVarName);
    var ffmpegCommand = Environment.GetEnvironmentVariable(ffmpegCommandVarName);

    if (String.IsNullOrEmpty(directoryPath))
    {
        Log($"Env. variable {directoryVarName} is null or empty");
        return (false, default, default);
    }

    if (String.IsNullOrEmpty(ffmpegCommand))
    {
        Log($"Env. variable {ffmpegCommandVarName} is null or empty. Will use default:");
        Log(defaultFfmpegCommand);
        ffmpegCommand = defaultFfmpegCommand;
    }

    return (true, directoryPath, ffmpegCommand);
}

static void Log(string message) => Console.WriteLine(message);