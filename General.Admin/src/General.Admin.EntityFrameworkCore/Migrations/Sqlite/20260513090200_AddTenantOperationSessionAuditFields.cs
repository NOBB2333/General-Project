using System;
using General.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(AdminDbContext))]
    [Migration("20260513090200_AddTenantOperationSessionAuditFields")]
    public partial class AddTenantOperationSessionAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImpersonatedUserId",
                table: "AppRequestAuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpersonatedUserName",
                table: "AppRequestAuditLogs",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperationSessionId",
                table: "AppRequestAuditLogs",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OperationTenantId",
                table: "AppRequestAuditLogs",
                type: "TEXT",
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
