using CommandLine;

namespace EFSeed.Cli;

public class Options
{
    [Option('n', "name", Required = false, HelpText = "Name of the seed")]
    public string Name { get; set; } = "SeedName";

    [Option("no-build", Required = false)]
    public bool NoBuild { get; set; }
    [Option("project", HelpText = "The project to use. If not specified, the current directory is used.")]
    public string? Project { get; set; }
}
