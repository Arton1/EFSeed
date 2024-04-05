using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Clearing;

internal class ClearScriptGenerator
{
    private readonly DbContext _db;

    public ClearScriptGenerator(DbContext db)
    {
        _db = db;
    }

    public string Generate()
    {

        // Implement it like Respawn does, but by using DbContext.Model instead
        // Ensure proper order of deletion, so that foreign key constraints are not violated
        // Take into account cyclic dependencies
        return "";
    }
}
