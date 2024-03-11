using EFSeed.Cli.Loading;
using EFSeed.Core;
using EFSeed.Core.StatementGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFSeed.Cli.Generate;

public class GenerateCommand(GenerateOptions options) : ICommand
{

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEntitiesStatementGeneratorFactory, EntitiesInsertStatementGeneratorFactory>();
        services.AddSingleton<EfSeeder>();
        var typesExtractor = new ProjectTypesExtractor();
        services.AddSingleton(typesExtractor);
        var dbContextLoader = new DbContextLoader(typesExtractor);
        var dbContext = dbContextLoader.Load(options);
        services.AddSingleton(dbContext);
        services.AddSingleton<DatabaseSeedLoader>();
    }

    public async Task<int> Run(IServiceProvider services)
    {
        var seeder = services.GetRequiredService<EfSeeder>();
        var context = services.GetRequiredService<DbContext>();
        var seedLoader = services.GetRequiredService<DatabaseSeedLoader>();
        var seed = seedLoader.Load(options);
        var script = seeder.CreateSeedScript(context, seed);
        await Console.Out.WriteAsync(script);
        return 0;
    }
}
