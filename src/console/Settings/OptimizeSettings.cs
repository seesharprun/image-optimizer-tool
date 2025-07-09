using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ImageOptimizer.Console.Settings;

internal sealed class OptimizeSettings : CommandSettings
{
    [Description("The path to optimize images in.")]
    [CommandOption("-p|--path <PATH>")]
    public DirectoryInfo? Path { get; init; }

    [Description("The file to optimize.")]
    [CommandOption("-f|--file <FILE>")]
    public FileInfo? File { get; init; }

    [Description("The file types to optimize. Defaults to .png and .jpg.")]
    [CommandOption("-t|--types <TYPES>")]
    public string[] FileTypes { get; init; } = { ".png", ".jpg" };

    [Description("Whether to keep the transparency of the image. Defaults to false.")]
    [CommandOption("-o|--opacity-transparent")]
    public bool Transparent { get; init; } = false;

    public FileSystemInfo Source => _internalSource ?? new DirectoryInfo(Directory.GetCurrentDirectory());

    private FileSystemInfo? _internalSource => File is not null ? File : Path;

    public override ValidationResult Validate() =>
        Source switch
        {
            null => ValidationResult.Error("A file or path is required."),
            { Exists: false } => ValidationResult.Error("The specified file or path does not exist."),
            _ => ValidationResult.Success()
        };
}