using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace General.Admin.EntityFrameworkCore;

public sealed class SqlitePragmaConnectionInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ConfigureConnection(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await ConfigureConnectionAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void ConfigureConnection(DbConnection connection)
    {
        if (connection is not SqliteConnection sqliteConnection)
        {
            return;
        }

        using var command = sqliteConnection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA busy_timeout=30000;";
        command.ExecuteNonQuery();
    }

    private static async Task ConfigureConnectionAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is not SqliteConnection sqliteConnection)
        {
            return;
        }

        await using var command = sqliteConnection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL; PRAGMA busy_timeout=30000;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
