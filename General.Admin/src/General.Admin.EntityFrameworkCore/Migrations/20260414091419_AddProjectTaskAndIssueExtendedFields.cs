using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTaskAndIssueExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractClause",
                table: "AppPhaseOneProjectTasks",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeveloperOwnerName",
                table: "AppPhaseOneProjectTasks",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductOwnerName",
                table: "AppPhaseOneProjectTasks",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TesterOwnerName",
                table: "AppPhaseOneProjectTasks",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeveloperOwnerName",
                table: "AppPhaseOneProjectIssues",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductOwnerName",
                table: "AppPhaseOneProjectIssues",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequirementTitle",
                table: "AppPhaseOneProjectIssues",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TesterOwnerName",
                table: "AppPhaseOneProjectIssues",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractClause",
                table: "AppPhaseOneProjectTasks");

            migrationBuilder.DropColumn(
                name: "DeveloperOwnerName",
                table: "AppPhaseOneProjectTasks");

            migrationBuilder.DropColumn(
                name: "ProductOwnerName",
                table: "AppPhaseOneProjectTasks");

            migrationBuilder.DropColumn(
                name: "TesterOwnerName",
                table: "AppPhaseOneProjectTasks");

            migrationBuilder.DropColumn(
                name: "DeveloperOwnerName",
                table: "AppPhaseOneProjectIssues");

            migrationBuilder.DropColumn(
                name: "ProductOwnerName",
                table: "AppPhaseOneProjectIssues");

            migrationBuilder.DropColumn(
                name: "RequirementTitle",
                table: "AppPhaseOneProjectIssues");

            migrationBuilder.DropColumn(
                name: "TesterOwnerName",
                table: "AppPhaseOneProjectIssues");
        }
    }
}
