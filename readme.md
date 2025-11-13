# Image Parser

A .NET command-line tool for optimizing image files. This tool reduces file sizes while maintaining quality by compressing PNG and JPG images in single files or entire directories.

## Arguments

| Short Argument | Full Argument | Description | Notes |
| --- | --- | --- | --- |
| `-f` | `--file` | Name of single file to optimize | |
| `-p` | `--path` | Directory containing multiple files to optimize | |
| `-t` | `--types` | List of file types to match | *Only relevant for directories and defaults to `.png` and `.jpg`* |
| `-h` | `--help` | Renders help metadata | |

> [!TIP]
> You should only specify `--file` or `--path`. If you specify both, `--file` takes priority.

## Prerequisites

- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)

## Dynamically run package

Run the tool directly without installation using `dnx`:

```shell
dnx SeeSharpRun.ImageOptimizer --file "<file>"
dnx SeeSharpRun.ImageOptimizer --path "<directory>"
dnx SeeSharpRun.ImageOptimizer --path "<directory>" --types ".png" --types ".jpg"
```

## Install package

Install the tool globally on your machine:

```shell
dotnet tool install --global SeeSharpRun.ImageOptimizer
```

After installation, run using `dotnet tool run`:

```shell
dotnet tool run SeeSharpRun.ImageOptimizer --file "<file>"
dotnet tool run SeeSharpRun.ImageOptimizer --path "<directory>"
dotnet tool run SeeSharpRun.ImageOptimizer --path "<directory>" --types ".png" --types ".jpg"
```

Or use the shorter command alias:

```shell
imageoptimize --file "<file>"
imageoptimize --path "<directory>"
imageoptimize --path "<directory>" --types ".png" --types ".jpg"
```

## Run in dev

For development and testing, run directly from source:

```shell
dotnet run tool.cs -- --file "<file>"
dotnet run tool.cs -- --path "<directory>"
dotnet run tool.cs -- --path "<directory>" --types ".png" --types ".jpg"
```
