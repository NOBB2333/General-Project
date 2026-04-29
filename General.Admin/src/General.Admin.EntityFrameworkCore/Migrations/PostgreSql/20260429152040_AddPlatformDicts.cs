using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class AddPlatformDicts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppDictData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DictTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    TagColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_AppDictData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppDictTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Remark = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppDictTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDictData_DictTypeId_IsEnabled_Sort",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "IsEnabled", "Sort" });

            migrationBuilder.CreateIndex(
                name: "IX_AppDictData_DictTypeId_Value",
                table: "AppDictData",
                columns: new[] { "DictTypeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Code",
                table: "AppDictTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppDictTypes_Sort",
                table: "AppDictTypes",
                column: "Sort");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDictData");

            migrationBuilder.DropTable(
                name: "AppDictTypes");
        }
    }
}
