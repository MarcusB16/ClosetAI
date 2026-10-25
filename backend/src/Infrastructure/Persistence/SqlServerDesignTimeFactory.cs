using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClosetAPI.Infrastructure.Persistence;

public class SqlServerDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerClosetDbContext>
{
    public SqlServerClosetDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SqlServerClosetDbContext>()
            .UseSqlServer("Server=localhost;Database=ClosetAI;User Id=sa;Password=design-time;TrustServerCertificate=True")
            .Options;
        return new SqlServerClosetDbContext(options);
    }
}
