using CommandLine;

namespace EFSeed.Cli.Generate;

[Verb("generate", HelpText = "Generates seed data." )]
public class GenerateOptions : Options
{
    [Option("no-build", Required = false)]
    public bool NoBuild { get; set; }
    [Option("project", HelpText = "The project to use. If not specified, the current directory is used.")]
    public string? Project { get; set; }
}
