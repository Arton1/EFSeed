using CommandLine;

namespace EFSeed.Cli;

public class Options
{
    [Option("project", HelpText = "Path to project. If not specified, the current directory is used.")]
    public string? Project { get; set; }
}
