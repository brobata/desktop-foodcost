using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dfc.Data.LocalDatabase;

public class DfcDbContextFactory : IDesignTimeDbContextFactory<DfcDbContext>
{
    public DfcDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DfcDbContext>();

        // Use a temporary database path for migrations
        optionsBuilder.UseSqlite("Data Source=freecost_temp.db");

        return new DfcDbContext(optionsBuilder.Options);
    }
}
