using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddFileStorageSourcesAndSchedulerOps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BucketName",
                table: "AppPlatformFiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "AppPlatformFiles",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "AppPlatformFiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "AppPlatformFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StorageSourceId",
                table: "AppPlatformFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppFileStorageSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessKeyId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BucketName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CustomDomain = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EncryptedSecret = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PathTemplate = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RootPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    UseSsl = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_AppFileStorageSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_BusinessType_BusinessId",
                table: "AppPlatformFiles",
                columns: new[] { "BusinessType", "BusinessId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPlatformFiles_StorageSourceId",
                table: "AppPlatformFiles",
                column: "StorageSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_IsDefault",
                table: "AppFileStorageSources",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_Name",
                table: "AppFileStorageSources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppFileStorageSources_ProviderName_IsEnabled",
                table: "AppFileStorageSources",
                columns: new[] { "ProviderName", "IsEnabled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppFileStorageSources");

            migrationBuilder.DropIndex(
                name: "IX_AppPlatformFiles_BusinessType_BusinessId",
                table: "AppPlatformFiles");

            migrationBuilder.DropIndex(
                name: "IX_AppPlatformFiles_StorageSourceId",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "BucketName",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "AppPlatformFiles");

            migrationBuilder.DropColumn(
                name: "StorageSourceId",
                table: "AppPlatformFiles");
        }
    }
}
