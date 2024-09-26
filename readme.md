# Image Parser

TODO

## Arguments

| Short Argument | Full Argument | Description | Notes |
| --- | --- | --- | --- |
| `-f` | `--file` | Name of single file to optimize | |
| `-p` | `--path` | Directory containing multiple files to optimize | |
| `-t` | `--types` | List of file types to match | *Only relevant for directories and defaults to `.png` and `.jpg`* |
| `-h` | `--help` | Renders help metadata | |

> [!TIP]
> You should only specify `--file` or `--path`. If you specify both, `--file` takes priority.

## Run in dev

TODO

```shell
cd /src/console

dotnet run -- --file "<file>"
dotnet run -- --path "<directory>"
dotnet run -- --path "<directory>" --types ".png" --types ".jpg"
```

## Package and install

TODO

```shell
cd /src/console

dotnet pack
dotnet tool install --global --add-source .\nupkg\ ImageOptimizer.Tool

imageoptimize --file "<file>"
imageoptimize --path "<directory>"
imageoptimize --path "<directory>" --types ".jpg"
```
