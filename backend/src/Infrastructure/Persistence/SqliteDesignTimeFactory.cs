using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClosetAPI.Infrastructure.Persistence;

public class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteClosetDbContext>
{
    public SqliteClosetDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqliteClosetDbContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;
        return new SqliteClosetDbContext(options);
    }
}
