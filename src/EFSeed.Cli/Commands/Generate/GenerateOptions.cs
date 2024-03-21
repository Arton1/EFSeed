using CommandLine;
using EFSeed.Core.StatementGenerators;

namespace EFSeed.Cli.Commands.Generate;

[Verb("generate", HelpText = "Generates sql script." )]
public class GenerateOptions : Options
{
    [Option("mode", HelpText = $"Generation mode to use. Available modes: insert, merge.", Required = true)]
    public GenerationMode Mode { get; set; }
    [Option("no-build", Required = false, HelpText = "Do not build the project before generating the script.")]
    public bool NoBuild { get; set; }
}
