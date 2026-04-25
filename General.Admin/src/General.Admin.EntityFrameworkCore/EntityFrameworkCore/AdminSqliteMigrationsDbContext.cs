using Microsoft.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore;

public class AdminSqliteMigrationsDbContext : AdminDbContextBase<AdminSqliteMigrationsDbContext>
{
    public AdminSqliteMigrationsDbContext(DbContextOptions<AdminSqliteMigrationsDbContext> options)
        : base(options)
    {
    }
}
