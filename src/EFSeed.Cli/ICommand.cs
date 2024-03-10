using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli;

public interface ICommand
{
    void ConfigureServices(IServiceCollection services);
    int Run(IServiceProvider services);
}
