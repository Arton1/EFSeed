using CommandLine;
using EFSeed.Core.StatementGenerators;

namespace EFSeed.Cli.Commands.Generate;

[Verb("generate", HelpText = "Generates seed data." )]
public class GenerateOptions : Options
{
    [Option("no-build", Required = false)]
    public bool NoBuild { get; set; }
    [Option("mode", HelpText = $"The generation mode to use. Possible values: insert, merge. If not specified, insert mode will be used.")]
    public GenerationMode? Mode { get; set; }
}
