using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class SqlServerClosetDbContext(DbContextOptions<SqlServerClosetDbContext> options) : ClosetDbContext(options);
