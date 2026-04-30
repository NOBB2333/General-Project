using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddNotificationUnreadIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppUserNotifications_UserId_IsRemoved_IsRead",
                table: "AppUserNotifications",
                columns: new[] { "UserId", "IsRemoved", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUserNotifications_UserId_IsRemoved_IsRead",
                table: "AppUserNotifications");
        }
    }
}
