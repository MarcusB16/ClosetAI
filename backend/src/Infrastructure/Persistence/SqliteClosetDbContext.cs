using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class SqliteClosetDbContext(DbContextOptions<SqliteClosetDbContext> options) : ClosetDbContext(options);
