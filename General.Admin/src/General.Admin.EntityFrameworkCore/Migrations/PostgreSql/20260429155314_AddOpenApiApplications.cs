using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EncryptedSecret = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Scopes = table.Column<string>(type: "TEXT", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
