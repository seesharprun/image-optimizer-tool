using System.Diagnostics.CodeAnalysis;
using Humanizer;
using ImageMagick;
using ImageOptimizer.Console.Models;
using ImageOptimizer.Console.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using Output = Spectre.Console.AnsiConsole;

namespace ImageOptimizer.Console.Commands;

internal sealed class OptimizeCommand : AsyncCommand<OptimizeSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext _, [NotNull] OptimizeSettings settings)
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