using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    public partial class AddTenantLedgerAndForceLogout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdminUserId",
                table: "AppTenantAuthorizations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remark",
                table: "AppTenantAuthorizations",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ForceLogoutAfter",
                table: "AppUserProfiles",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "AppTenantAuthorizations");

            migrationBuilder.DropColumn(
                name: "Remark",
                table: "AppTenantAuthorizations");

            migrationBuilder.DropColumn(
                name: "ForceLogoutAfter",
                table: "AppUserProfiles");
        }
    }
}
