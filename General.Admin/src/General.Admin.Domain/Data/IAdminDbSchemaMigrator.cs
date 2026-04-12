using System.Threading.Tasks;

namespace General.Admin.Data;

public interface IAdminDbSchemaMigrator
{
    Task MigrateAsync();
}
