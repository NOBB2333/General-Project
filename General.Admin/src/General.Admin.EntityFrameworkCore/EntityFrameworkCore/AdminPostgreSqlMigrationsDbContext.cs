using Microsoft.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore;

public class AdminPostgreSqlMigrationsDbContext : AdminDbContextBase<AdminPostgreSqlMigrationsDbContext>
{
    public AdminPostgreSqlMigrationsDbContext(DbContextOptions<AdminPostgreSqlMigrationsDbContext> options)
        : base(options)
    {
    }
}
