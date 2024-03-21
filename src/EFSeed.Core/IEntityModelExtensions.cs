using Microsoft.EntityFrameworkCore.Metadata;

namespace EFSeed.Core;

internal static class IEntityTypeExtensions
{
    public static bool HasIdentityInsert(this IEntityType entityModel) =>
        entityModel.FindPrimaryKey()?.Properties.Any(p => p.ValueGenerated == ValueGenerated.OnAdd) == true;
}
