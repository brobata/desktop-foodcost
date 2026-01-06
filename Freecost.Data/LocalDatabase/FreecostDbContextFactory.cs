using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Freecost.Data.LocalDatabase;

public class FreecostDbContextFactory : IDesignTimeDbContextFactory<FreecostDbContext>
{
    public FreecostDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FreecostDbContext>();

        // Use a temporary database path for migrations
        optionsBuilder.UseSqlite("Data Source=freecost_temp.db");

        return new FreecostDbContext(optionsBuilder.Options);
    }
}
