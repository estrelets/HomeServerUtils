using Converter;

var variables = Variables.Read();

var (inputDirectory, outputDirectory, extensions, _ ,_) = variables;

var files = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
CreateDirectories(inputDirectory, outputDirectory, files);

foreach (var file in files)
{
    if (IsConvertable(extensions, file))
    {
        await Convert(variables, file);   
    }
    else
    {
        Copy(variables, file);
    }
}

static void CreateDirectories(string inputDirectory, string outputDirectory, string[] files)
{
    var directories = files
        .Select(file => GetDestinationPath(inputDirectory, outputDirectory, file))
        .Select(Path.GetDirectoryName)
        .Distinct();

    foreach (var dir in directories)
    {
        Directory.CreateDirectory(dir);
    }
}

static async Task Convert(Variables vars, string file)
{
    var dstFile = GetDestinationPath(vars.Input, vars.Output, file);

    if (File.Exists(dstFile))
    {
        Console.WriteLine($"File '{dstFile}' already exists");
        return;
    }
    
    var converter = new GenericConverter(vars.FFMpeg, vars.FFMpegArgs);
    await converter.Convert(file, dstFile, default);
}

static void Copy(Variables vars, string file)
{
    var dstFile = GetDestinationPath(vars.Input, vars.Output, file);
    try
    {
        File.Copy(file, dstFile, false);
    }
    catch
    {
        //ignore
    }
}

static string GetDestinationPath(string inputDirectory, string outputDirectory, string file)
{
    var relativePath = Path.GetRelativePath(inputDirectory, file);
    return Path.Combine(outputDirectory, relativePath);
}

static bool IsConvertable(string[] extensions, string file)
{
    var extension = Path.GetExtension(file);
    return extensions.Any(e => extension.Equals(e, StringComparison.InvariantCultureIgnoreCase));
}