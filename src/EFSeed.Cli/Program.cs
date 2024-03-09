using CommandLine;
using EFSeed.Cli;

var result = Parser.Default.ParseArguments<Options>(args);

if (result.Tag != ParserResultType.Parsed)
{
    return 1;
}

var options = result.Value;

var dbContext = new DbContextLoader().LoadDbContext(options, args);

return 0;
