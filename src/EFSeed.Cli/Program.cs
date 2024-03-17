using CommandLine;
using EFSeed.Cli;
using EFSeed.Cli.Commands.Clear;
using EFSeed.Cli.Commands.Generate;
using EFSeed.Cli.Load;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.None);
    });

var options = new Parser(with =>
{
    with.AutoHelp = true;
    with.AutoVersion = true;
    with.HelpWriter = Console.Out;
    with.CaseInsensitiveEnumValues = true;
    with.CaseSensitive = false;
}).Parse(args) as Options;
if (options == null)
{
    return 1;
}
ICommand command = options switch
{
    GenerateOptions generateOptions => new GenerateCommand(generateOptions),
    ClearOptions clearOptions => new ClearCommand(clearOptions),
    _ => throw new NotSupportedException()
};

builder.ConfigureServices(services =>
{
    command.ConfigureServices(services);
});

using var host = builder.Build();
host.Start();

return await command.Run(host.Services);


