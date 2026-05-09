using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddHostTenantOperationAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HostOperatorUserId",
                table: "AppRequestAuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostOperatorUserName",
                table: "AppRequestAuditLogs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHostTenantOperation",
                table: "AppRequestAuditLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppRequestAuditLogs_IsHostTenantOperation_ExecutionTime",
                table: "AppRequestAuditLogs",
                columns: new[] { "IsHostTenantOperation", "ExecutionTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppRequestAuditLogs_IsHostTenantOperation_ExecutionTime",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "HostOperatorUserId",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "HostOperatorUserName",
                table: "AppRequestAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsHostTenantOperation",
                table: "AppRequestAuditLogs");
        }
    }
}
