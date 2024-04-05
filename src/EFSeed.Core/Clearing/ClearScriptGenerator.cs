using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Clearing;

// https://www.jimmybogard.com/how-respawn-works/
internal class ClearScriptGenerator
{
    private readonly DbContext _db;

    public ClearScriptGenerator(DbContext db)
    {
        _db = db;
    }

    public string Generate()
    {
        var model = _db.Model;
        var entities = model.GetEntityTypes().ToList();
        var deletionSorter = new EntitiesDeletionSorter();
        var sortedEntities = deletionSorter.SortForDeletion(entities);

        return "";
    }
}
