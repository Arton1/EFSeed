using CommandLine;
using EFSeed.Cli;
using EFSeed.Cli.Generate;
using EFSeed.Cli.Loading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(LogLevel.None);
    });

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

builder.ConfigureServices(services =>
{
    services.AddSingleton<ProjectTypesExtractor>();
    command.ConfigureServices(services);
});

using var host = builder.Build();
host.Start();

return await command.Run(host.Services);


