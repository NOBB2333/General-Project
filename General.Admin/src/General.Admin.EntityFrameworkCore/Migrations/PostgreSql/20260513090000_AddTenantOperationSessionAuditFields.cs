using System;
using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    [DbContext(typeof(AdminPostgreSqlMigrationsDbContext))]
    [Migration("20260513090000_AddTenantOperationSessionAuditFields")]
    public partial class AddTenantOperationSessionAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImpersonatedUserId",
                table: "AppRequestAuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpersonatedUserName",
                table: "AppRequestAuditLogs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationSessionId",
                table: "AppRequestAuditLogs",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationTenantId",
                table: "AppRequestAuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_OperationSessionId_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "OperationSessionId", "ExecutionTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRequestAuditLogs_OperationSessionId_ExecutionTime",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "ImpersonatedUserId",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "ImpersonatedUserName",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "OperationSessionId",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "OperationTenantId",
                table: "AppRequestAuditLogs");
        }
    }
}
