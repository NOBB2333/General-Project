using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace General.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddPhaseTwoBusinessManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessBudgetExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdjustedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BudgetCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExecutedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    VarianceAmount = table.Column<decimal>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessBudgetExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessChains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChainCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessChains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CounterpartyName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsRevenueContract = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SignDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessForecastHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Metric = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RelatedCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessForecastHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessProcurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProcurementCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SignDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SourceChangeCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Stage = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessProcurements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPhaseOneBusinessReceivables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    LinkedContractCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    PlannedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceivedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    ReceivableCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
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
                    table.PrimaryKey("PK_AppPhaseOneBusinessReceivables", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessBudgetExecutions_BudgetCode",
                table: "AppPhaseOneBusinessBudgetExecutions",
                column: "BudgetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessBudgetExecutions_ProjectId_SortOrder",
                table: "AppPhaseOneBusinessBudgetExecutions",
                columns: new[] { "ProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessChains_ChainCode",
                table: "AppPhaseOneBusinessChains",
                column: "ChainCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessChains_ProjectId_Stage_Status",
                table: "AppPhaseOneBusinessChains",
                columns: new[] { "ProjectId", "Stage", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessContracts_ContractCode",
                table: "AppPhaseOneBusinessContracts",
                column: "ContractCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessContracts_ProjectId_IsRevenueContract_SignDate",
                table: "AppPhaseOneBusinessContracts",
                columns: new[] { "ProjectId", "IsRevenueContract", "SignDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessForecastHistories_ProjectId_Metric_CreationTime",
                table: "AppPhaseOneBusinessForecastHistories",
                columns: new[] { "ProjectId", "Metric", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessProcurements_ProcurementCode",
                table: "AppPhaseOneBusinessProcurements",
                column: "ProcurementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessProcurements_ProjectId_SignDate",
                table: "AppPhaseOneBusinessProcurements",
                columns: new[] { "ProjectId", "SignDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessReceivables_ProjectId_PlannedDate",
                table: "AppPhaseOneBusinessReceivables",
                columns: new[] { "ProjectId", "PlannedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AppPhaseOneBusinessReceivables_ReceivableCode",
                table: "AppPhaseOneBusinessReceivables",
                column: "ReceivableCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessBudgetExecutions");

            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessChains");

            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessContracts");

            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessForecastHistories");

            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessProcurements");

            migrationBuilder.DropTable(
                name: "AppPhaseOneBusinessReceivables");
        }
    }
}
