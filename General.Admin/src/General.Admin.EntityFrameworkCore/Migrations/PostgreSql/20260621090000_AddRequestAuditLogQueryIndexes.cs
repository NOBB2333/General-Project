using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260621090000_AddRequestAuditLogQueryIndexes")]
    public partial class AddRequestAuditLogQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_Category_HttpMethod_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "Category", "HttpMethod", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_HasException_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "HasException", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_HttpMethod_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "HttpMethod", "ExecutionTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRequestAuditLogs_Category_HttpMethod_ExecutionTime",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestAuditLogs_HasException_ExecutionTime",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AppRequestAuditLogs_HttpMethod_ExecutionTime",
                table: "AppRequestAuditLogs");
        }
    }
}
