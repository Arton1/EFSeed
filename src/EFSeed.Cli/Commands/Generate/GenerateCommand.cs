using EFSeed.Cli.Generate;
using EFSeed.Cli.Load;
using EFSeed.Core;
using EFSeed.Core.StatementGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli.Commands.Generate;

public class GenerateCommand(GenerateOptions options) : ICommand
{

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEntitiesStatementGeneratorFactory, EntitiesInsertStatementGeneratorFactory>();
        services.AddSingleton<EfSeeder>();
        var dependenciesLoader = ProjectDependenciesLoader.Create(new ProjectAssemblyLoaderOptions()
        {
            Path = options.Project,
            NoBuild = options.NoBuild,
        });
        var dbContext = dependenciesLoader.CreateDbContext();
        services.AddSingleton(dbContext);
        var seed = dependenciesLoader.CreateDatabaseSeed();
        services.AddSingleton(seed);
    }

    public async Task<int> Run(IServiceProvider services)
    {
        var seeder = services.GetRequiredService<EfSeeder>();
        var context = services.GetRequiredService<DbContext>();
        var seed = services.GetRequiredService<IDatabaseSeed>();
        var seedBuilder = new SeedBuilder();
        seed.Seed(seedBuilder);
        var seedDefinition = seedBuilder.Build();
        var script = seeder.CreateSeedScript(context, seedDefinition.Seed);
        await Console.Out.WriteAsync(script);
        return 0;
    }
}
