using CommandLine;

namespace EFSeed.Cli;

public class Options
{
    [Option("project", HelpText = "The project to use. If not specified, the current directory is used.")]
    public string? Project { get; set; }
}
