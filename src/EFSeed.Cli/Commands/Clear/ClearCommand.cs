using EFSeed.Cli.Load;
using EFSeed.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli.Commands.Clear;

public class ClearCommand(ClearOptions options) : ICommand
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<EfSeederBuilder>();
        var dependenciesLoader = ProjectDependenciesLoader.Create(new ProjectAssemblyLoaderOptions
        {
            Path = options.Project,
            NoBuild = true
        });
        var dbContext = dependenciesLoader.CreateDbContext();
        services.AddSingleton(dbContext);
        // var seed = dependenciesLoader.CreateDatabaseSeed();
        // services.AddSingleton(seed);
    }

    public async Task<int> Run(IServiceProvider services)
    {
        var seederBuilder = services.GetRequiredService<EfSeederBuilder>();
        var context = services.GetRequiredService<DbContext>();
        // var seed = services.GetRequiredService<IDatabaseSeed>();
        // var seedDefinition = seed.GenerateDefinition();
        var seeder = seederBuilder
            .WithDbContext(context)
            .Build();
        await seeder.ClearDatabase();
        return 0;
    }
}
