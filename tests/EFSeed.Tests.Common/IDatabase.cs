using Microsoft.EntityFrameworkCore;

namespace EFSeed.Core.Tests.Common;

public interface IDatabase
{
    DbContext CreateCleanDbContext();
}
