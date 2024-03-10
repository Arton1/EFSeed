using System.Reflection;
using CommandLine;
using EFSeed.Cli.Generate;

namespace EFSeed.Cli;

public static class ParserExtensions
{
    public static Object? Parse(this Parser parser, string[] args)
    {
        var verbs = LoadVerbs();
        var result = parser.ParseArguments(args, verbs);
        return result.Tag == ParserResultType.Parsed ? result.Value : null;
    }

    private static Type[] LoadVerbs() =>
        Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();
}
