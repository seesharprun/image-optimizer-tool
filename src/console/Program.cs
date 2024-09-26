using ImageParser.Console.Commands;
using Spectre.Console.Cli;

await new CommandApp<OptimizeCommand>().RunAsync(args);
