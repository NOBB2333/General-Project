using Microsoft.EntityFrameworkCore;

namespace General.Admin.EntityFrameworkCore;

public class AdminMySqlMigrationsDbContext : AdminDbContextBase<AdminMySqlMigrationsDbContext>
{
    public AdminMySqlMigrationsDbContext(DbContextOptions<AdminMySqlMigrationsDbContext> options)
        : base(options)
    {
    }
}
