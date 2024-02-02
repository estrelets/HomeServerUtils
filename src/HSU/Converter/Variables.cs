namespace Converter;

public class Variables
{
    public void Deconstruct(out string input, out string output, out string[] extensions, out string ffmpeg, out string ffmpegArgs)
    {
        input = Input;
        output = Output;
        extensions = Extensions;
        ffmpeg = FFMpeg;
        ffmpegArgs = FFMpegArgs;
    }

    public required string Input { get; init; }
    public required string Output { get; init; }
    public required string[] Extensions { get; init; }
    public required string FFMpeg { get; init; }
    public required string FFMpegArgs { get; init; }

    public static Variables Read()
    {
        var inputDirectory = Environment.GetEnvironmentVariable("INPUT_DIRECTORY");
        var outputDirectory = Environment.GetEnvironmentVariable("OUTPUT_DIRECTORY");

        void ValidateDirectory(string path, string name)
        {
            if(String.IsNullOrEmpty(path)) throw new Exception($"""{name} cannot be empty""");
            if (!Directory.Exists(path)) throw new Exception($"Directory {path} not found");
        }

        ValidateDirectory(inputDirectory, "INPUT_DIRECTORY");
        ValidateDirectory(outputDirectory, "OUTPUT_DIRECTORY");

        var extensionString = Environment.GetEnvironmentVariable("MEDIA_FILES_EXTENSIONS");
        var extensions = extensionString
            ?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToLowerInvariant())
            .ToArray();
        
        if (extensions == null || extensions.Length == 0)
        {
            extensions = new[] { ".mp4", ".mov", ".m4v", ".mkv"};
            Console.WriteLine("Extensions is empty, will use default");
        }

        var ffmpeg = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        if (String.IsNullOrEmpty(ffmpeg))
        {
            throw new Exception("FFMPEG_PATH cannot be empty");
        }

        if (!File.Exists(ffmpeg))
        {
            throw new Exception($"File {ffmpeg} not found");
        }
        
        var ffmpegArgs = Environment.GetEnvironmentVariable("FFMPEG_ARGS");
        if (String.IsNullOrEmpty(ffmpegArgs))
        {
            throw new Exception("FFMPEG_ARGS cannot be empty");
        }

        return new Variables
        {
            Input = inputDirectory,
            Output = outputDirectory,
            Extensions = extensions,
            FFMpeg = ffmpeg,
            FFMpegArgs = ffmpegArgs
        };
    }
}