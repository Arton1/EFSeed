using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli.Generate;

public class GenerateCommand(GenerateOptions options) : ICommand
{

    public void ConfigureServices(IServiceCollection services)
    {

    }

    public int Run(IServiceProvider services)
    {
        Console.WriteLine("Generate command");
        return 0;
    }
}
