using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli;

public interface ICommand
{
    void ConfigureServices(IServiceCollection services);
    Task<int> Run(IServiceProvider services);
}
