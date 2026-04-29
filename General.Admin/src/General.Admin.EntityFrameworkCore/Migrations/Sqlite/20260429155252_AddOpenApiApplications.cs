using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddOpenApiApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppOpenApiApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EncryptedSecret = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppOpenApiApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiApplications_AppId",
                table: "AppOpenApiApplications",
                column: "AppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppOpenApiApplications_IsEnabled",
                table: "AppOpenApiApplications",
                column: "IsEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppOpenApiApplications");
        }
    }
}
