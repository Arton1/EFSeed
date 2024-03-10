using CommandLine;
using EFSeed.Cli;
using EFSeed.Cli.Generate;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateDefaultBuilder(args);

var options = Parser.Default.Parse(args);
if (options == null)
{
    return 1;
}
var command = options switch
{
    GenerateOptions generateOptions => new GenerateCommand(generateOptions),
    _ => throw new NotSupportedException()
};

builder.ConfigureServices(command.ConfigureServices);

using var host = builder.Build();
host.Start();

return command.Run(host.Services);


