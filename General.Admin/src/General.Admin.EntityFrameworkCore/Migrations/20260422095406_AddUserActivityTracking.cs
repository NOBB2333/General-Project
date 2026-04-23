using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddUserActivityTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "AppUserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSeenBrowser",
                table: "AppUserProfiles",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSeenDevice",
                table: "AppUserProfiles",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSeenIpAddress",
                table: "AppUserProfiles",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "LastSeenBrowser",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "LastSeenDevice",
                table: "AppUserProfiles");

            migrationBuilder.DropColumn(
                name: "LastSeenIpAddress",
                table: "AppUserProfiles");
        }
    }
}
