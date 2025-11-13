#:package Humanizer@2.14.1
#:package Magick.NET-Q8-AnyCPU@14.9.1
#:package Spectre.Console.Cli@0.53.0
#:package Spectre.Console@0.54.0

#:property TargetFrameworks=net8.0;net9.0;net10.0
#:property TargetFramework=net10.0
#:property RollForward=LatestMajor
#:property PublishAot=false
#:property PackAsTool=true
#:property AssemblyName=SeeSharpRun.ImageOptimizer
#:property ToolCommandName=imageoptimize
#:property PackageId=SeeSharpRun.ImageOptimizer
#:property Authors=SeeSharpRun
#:property Description=Command line to bulk optimize images for a specific image or path.
#:property PackageLicenseExpression=MIT
#:property PackageRequireLicenseAcceptance=false
#:property ProjectUrl=https://github.com/seesharprun/image-optimizer-tool#readme
#:property RepositoryUrl=https://github.com/seesharprun/image-optimizer-tool
#:property RepositoryType=git
#:property PackageIcon=icon.png
#:property PackageReadmeFile=readme.md

using System.Diagnostics.CodeAnalysis;
using Humanizer;
using ImageMagick;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using Output = Spectre.Console.AnsiConsole;

await new CommandApp<OptimizeCommand>().RunAsync(args);

internal sealed class OptimizeCommand : AsyncCommand<OptimizeSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext _, [NotNull] OptimizeSettings settings, CancellationToken cancellationToken)
    {
        Output.MarkupLine("[bold olive]Staring image optimization[/]");

        Table table = new();
        table.Expand();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[bold]Filename[/]", column => column.LeftAligned());
        table.AddColumn("[bold]Original Size[/]", column => column.RightAligned());
        table.AddColumn("[bold]Optimized Size[/]", column => column.RightAligned());

        Panel panel = new(
            settings.Source switch
            {
                FileInfo file => $"[maroon]Optimizing image located at {file.FullName}[/]",
                DirectoryInfo directory => $"[maroon]Optimizing all images located at {directory.FullName}[/]",
                _ => String.Empty
            }
        );
        panel.BorderColor(Color.Maroon);
        panel.Expand();
        panel.Border = BoxBorder.Rounded;
        Output.Write(panel);

        await Output.Live(table)
            .StartAsync(async context =>
            {
                List<Task<Result>> optimizationTasks = new();

                IEnumerable<Task<Result>> tasks = settings.Source switch
                {
                    FileInfo file => GetTasks(file, settings.Transparent),
                    DirectoryInfo directory => GetTasks(directory, settings.FileTypes, settings.Transparent),
                    _ => throw new InvalidOperationException("Invalid path type.")
                };

                optimizationTasks.AddRange(tasks);

                await Task.WhenAll(
                    optimizationTasks.ToArray().Select(task => task.ContinueWith(source =>
                    {
                        table.AddRow(
                            $"[bold]{source.Result.filename}[/]",
                            $"{source.Result.originalFileSize.Bytes().Humanize("#.##")}",
                            $"[italic]{source.Result.optimizedFileSize.Bytes().Humanize("#.##")}[/]"
                        );
                        context.Refresh();
                    }))
                );
            });

        Output.MarkupLine("[bold olive]Finalizing image optimization[/]");

        return 0;
    }

    private IEnumerable<Task<Result>> GetTasks(FileInfo file, bool transparent) =>
        new List<Task<Result>> { OptimizeFile(file.FullName, transparent) };

    private IEnumerable<Task<Result>> GetTasks(DirectoryInfo directory, IEnumerable<string> fileTypes, bool transparent) =>
        directory
            .GetFiles()
            .Where(file => fileTypes.Any(type => file.FullName.EndsWith(type, StringComparison.OrdinalIgnoreCase)))
            .Select(file => OptimizeFile(file.FullName, transparent));

    private async Task<Result> OptimizeFile(string filename, bool transparent)
    {
        long originalFileSize = new FileInfo(filename).Length;

        MagickImage image = new(filename);

        if (!transparent)
        {
            image.BackgroundColor = new MagickColor(MagickColors.White);
            image.Alpha(AlphaOption.Remove);
        }
        else
        {
            image.BackgroundColor = new MagickColor(MagickColors.Transparent);
            image.Alpha(AlphaOption.Set);
        }

        image.Quantize(new QuantizeSettings
        {
            Colors = 256
        });

        await image.WriteAsync(filename);

        long optimizedFileSize = new FileInfo(filename).Length;

        return new(filename, originalFileSize, optimizedFileSize);
    }
}

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

internal record Result(string filename, long originalFileSize, long optimizedFileSize);