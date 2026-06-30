using System;
using System.Collections.Generic;
using System.Text;
using General.Admin.Business;
using General.Admin.Project;
using Shouldly;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

public class BusinessQueryBuilderTests
{
    private readonly BusinessWarningBuilder _warningBuilder = new();
    private readonly BusinessQueryBuilder _builder;
    private readonly BusinessReportQueryBuilder _reportBuilder;
    private readonly BusinessReportExportBuilder _exportBuilder = new();

    public BusinessQueryBuilderTests()
    {
        _builder = new BusinessQueryBuilder(_warningBuilder);
        _reportBuilder = new BusinessReportQueryBuilder(_warningBuilder);
    }

    [Fact]
    public void BuildOverview_Should_Aggregate_Amounts_And_Build_Warning_Focus_Items()
    {
        var context = CreateContext();

        var overview = _builder.BuildOverview(
            context,
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        overview.ContractAmount.ShouldBe(1_500_000m);
        overview.CostAmount.ShouldBe(620_000m);
        overview.ReceivedAmount.ShouldBe(700_000m);
        overview.YearEndReceivableForecast.ShouldBe(1_300_000m);
        overview.ReceivableRiskProjectCount.ShouldBe(1);
        overview.TailClosingProjectCount.ShouldBe(1);
        overview.WarningItems.ShouldContain(item => item.Type == "回款风险");
        overview.WarningItems.ShouldContain(item => item.Type == "进度绩效风险");
        overview.FocusProjects.ShouldContain(item => item.ActionTab == "receivable");
    }

    [Fact]
    public void BuildProjectDetail_Should_Project_Full_Business_Chain_And_Mask_Sensitive_Amounts()
    {
        var context = CreateContext();
        var project = context.Projects[0];

        var detail = _builder.BuildProjectDetail(
            context,
            project,
            new BusinessQueryAccess(
                CanViewProfitAnalysis: false,
                CanViewSensitiveAmounts: false));

        detail.CanViewSensitiveAmounts.ShouldBeFalse();
        detail.Summary.ContractAmount.ShouldBeNull();
        detail.Summary.GrossProfitRate.ShouldBeNull();
        detail.Summary.SchedulePerformanceIndex.ShouldBe(0.8m);
        detail.Summary.CostPerformanceIndex.ShouldBe(0.67m);
        detail.Contracts.Count.ShouldBe(1);
        detail.Contracts[0].ContractCode.ShouldBe("REV-001");
        detail.Contracts[0].Amount.ShouldBeNull();
        detail.BudgetExecutions[0].AdjustedAmount.ShouldBeNull();
        detail.Procurements[0].Amount.ShouldBeNull();
        detail.Receivables[0].TotalAmount.ShouldBeNull();
        detail.Invoices[0].InvoiceCode.ShouldBe("INV-001");
        detail.Invoices[0].Amount.ShouldBeNull();
        detail.Acceptances[0].AcceptanceCode.ShouldBe("ACC-001");
        detail.BusinessChains[0].OwnerName.ShouldBe("项目经理");
        detail.ForecastHistories[0].Metric.ShouldBe("年末预计回款");
        detail.ForecastHistories[0].ChangedBy.ShouldBe("项目经理");
    }

    [Fact]
    public void BuildReport_Should_Filter_Contracts_And_Keep_Sensitive_Amounts_For_Privileged_Access()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                ContractType = "主合同",
                CounterpartyName = "客户甲"
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.ContractItems.Count.ShouldBe(1);
        report.ContractItems[0].ContractCode.ShouldBe("REV-001");
        report.ContractItems[0].Amount.ShouldBe(1_000_000m);
        report.ProjectItems.Count.ShouldBe(1);
        report.ProjectItems[0].BusinessCloseStatus.ShouldBe("经营收尾中");
        report.WarningItems.ShouldContain(item => item.Type == "回款风险");
    }

    [Fact]
    public void BuildReport_Should_Mask_Sensitive_Amounts_For_Unprivileged_Access()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput(),
            new BusinessQueryAccess(
                CanViewProfitAnalysis: false,
                CanViewSensitiveAmounts: false));

        report.ContractItems[0].Amount.ShouldBeNull();
        report.ProcurementItems[0].Amount.ShouldBeNull();
        report.ProjectItems[0].GrossProfitAmount.ShouldBeNull();
        report.ProjectItems[0].GrossProfitRate.ShouldBeNull();
    }

    [Fact]
    public void BuildReport_Should_Filter_Procurements_By_Supplier_And_Sign_Date()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                SignDateFrom = new DateTime(2026, 1, 1),
                SignDateTo = new DateTime(2026, 1, 31),
                SupplierName = "供应商甲"
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.ProcurementItems.Count.ShouldBe(1);
        report.ProcurementItems[0].ProcurementCode.ShouldBe("PROC-001");
        report.ProcurementItems[0].Amount.ShouldBe(300_000m);
    }

    [Fact]
    public void BuildReport_Should_Filter_Receivables_By_Status_And_Planned_Date()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                PlannedDateFrom = new DateTime(2026, 3, 1),
                PlannedDateTo = new DateTime(2026, 3, 31),
                ReceivableStatus = "待回款"
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.ReceivableItems.Count.ShouldBe(1);
        report.ReceivableItems[0].ReceivableCode.ShouldBe("REC-001");
        report.ReceivableItems[0].TotalAmount.ShouldBe(800_000m);
    }

    [Fact]
    public void BuildReport_Should_Filter_Invoices_By_Invoice_Date()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                PlannedDateFrom = new DateTime(2026, 2, 1),
                PlannedDateTo = new DateTime(2026, 2, 28)
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.InvoiceItems.Count.ShouldBe(1);
        report.InvoiceItems[0].InvoiceCode.ShouldBe("INV-001");
        report.InvoiceItems[0].Amount.ShouldBe(800_000m);
    }

    [Fact]
    public void BuildReport_Should_Filter_Acceptances_By_Acceptance_Date()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                PlannedDateFrom = new DateTime(2026, 4, 1),
                PlannedDateTo = new DateTime(2026, 4, 30)
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.AcceptanceItems.Count.ShouldBe(1);
        report.AcceptanceItems[0].AcceptanceCode.ShouldBe("ACC-001");
        report.AcceptanceItems[0].AcceptanceResult.ShouldBe("通过");
    }

    [Fact]
    public void BuildReport_Should_Filter_Warnings_By_Status_And_Level()
    {
        var context = CreateContext();

        var report = _reportBuilder.BuildReport(
            context,
            new BusinessReportQueryInput
            {
                BusinessCloseStatus = "经营收尾中",
                WarningLevel = ProjectPriorities.P0
            },
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));

        report.WarningItems.Count.ShouldBe(1);
        report.WarningItems.ShouldAllBe(item => item.Status == "经营收尾中");
        report.WarningItems.ShouldAllBe(item => item.Level == ProjectPriorities.P0);
        report.WarningItems.ShouldContain(item => item.Type == "成本绩效风险");
    }

    [Fact]
    public void BuildReportExport_Should_Use_Filtered_Report_And_Emit_Utf8_Csv()
    {
        var context = CreateContext();

        var input = new BusinessReportExportInput
        {
            LedgerKind = "contract",
            ContractType = "主合同",
            CounterpartyName = "客户甲"
        };
        var report = _reportBuilder.BuildReport(
            context,
            input,
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));
        var result = _exportBuilder.Build(report, input);

        result.FileName.ShouldStartWith("经营合同台账_");
        result.ContentType.ShouldBe("text/csv;charset=utf-8");
        var csv = Encoding.UTF8.GetString(result.Content);
        csv.ShouldStartWith('\uFEFF'.ToString());
        csv.ShouldContain("合同编号,合同名称,类型,所属项目,相对方,签订日期,金额,状态");
        csv.ShouldContain("REV-001,主合同,主合同,智慧园区项目,客户甲,2026-01-10,1000000,执行中");
        csv.ShouldNotContain("REV-002");
    }

    [Fact]
    public void BuildReportExport_Should_Export_Profit_Analysis_When_LedgerKind_Is_Profit()
    {
        var context = CreateContext();

        var input = new BusinessReportExportInput
        {
            LedgerKind = "profit",
            BusinessCloseStatus = "经营收尾中"
        };
        var report = _reportBuilder.BuildReport(
            context,
            input,
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));
        var result = _exportBuilder.Build(report, input);

        result.FileName.ShouldStartWith("经营利润分析_");
        var csv = Encoding.UTF8.GetString(result.Content);
        csv.ShouldContain("项目编号,项目名称,客户,项目经理,经营状态,毛利额,毛利率,年底预计回款,状态");
        csv.ShouldContain("PRJ-001,智慧园区项目,客户甲,项目经理,经营收尾中,500000,50%,800000,进行中");
        csv.ShouldNotContain("PRJ-002");
    }

    [Fact]
    public void BuildReportExport_Should_Use_Filtered_Warning_Report()
    {
        var context = CreateContext();

        var input = new BusinessReportExportInput
        {
            LedgerKind = "warning",
            BusinessCloseStatus = "经营收尾中",
            WarningLevel = ProjectPriorities.P0
        };
        var report = _reportBuilder.BuildReport(
            context,
            input,
            new BusinessQueryAccess(
                CanViewProfitAnalysis: true,
                CanViewSensitiveAmounts: true));
        var result = _exportBuilder.Build(report, input);

        var csv = Encoding.UTF8.GetString(result.Content);
        csv.ShouldContain("预警类型,等级,项目编号,项目名称,预警说明,责任人,经营状态,最近更新");
        csv.ShouldContain("成本绩效风险,P0,PRJ-001,智慧园区项目");
        csv.ShouldNotContain("回款风险");
        csv.ShouldNotContain("收尾风险");
    }

    private static BusinessContext CreateContext()
    {
        var projectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var otherProjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var managerUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var sponsorUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var organizationUnitId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var project = new ProjectEntity(
            projectId,
            "PRJ-001",
            "智慧园区项目",
            "园区",
            "软件项目",
            "客户甲",
            organizationUnitId,
            managerUserId,
            sponsorUserId,
            ProjectPriorities.P0,
            ProjectStatuses.InProgress,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 6, 30),
            true,
            "经营测试项目");
        var otherProject = new ProjectEntity(
            otherProjectId,
            "PRJ-002",
            "数据中台项目",
            "中台",
            "软件项目",
            "客户乙",
            organizationUnitId,
            managerUserId,
            sponsorUserId,
            ProjectPriorities.P1,
            ProjectStatuses.InProgress,
            new DateTime(2026, 2, 1),
            new DateTime(2026, 7, 31),
            false,
            "筛选排除项目");

        var contracts = new List<BusinessContract>
        {
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                projectId,
                "REV-001",
                "主合同",
                "主合同",
                "客户甲",
                1_000_000m,
                new DateTime(2026, 1, 10),
                "执行中",
                true),
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                otherProjectId,
                "REV-002",
                "其他合同",
                "主合同",
                "客户乙",
                500_000m,
                new DateTime(2026, 2, 10),
                "执行中",
                true)
        };
        var procurements = new List<BusinessProcurement>
        {
            new(
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                projectId,
                "PROC-001",
                "分包实施",
                "供应商甲",
                300_000m,
                new DateTime(2026, 1, 20),
                "执行",
                "执行中",
                "REV-001"),
            new(
                Guid.Parse("20000000-0000-0000-0000-000000000002"),
                otherProjectId,
                "PROC-002",
                "云资源采购",
                "供应商乙",
                120_000m,
                new DateTime(2026, 2, 20),
                "采购",
                "已完成",
                "REV-002")
        };
        var budgetExecutions = new List<BusinessBudgetExecution>
        {
            new(
                Guid.Parse("30000000-0000-0000-0000-000000000001"),
                projectId,
                "BGT-001",
                "实施成本",
                450_000m,
                200_000m,
                -250_000m,
                1)
        };
        var businessChains = new List<BusinessChain>
        {
            new(
                Guid.Parse("50000000-0000-0000-0000-000000000001"),
                projectId,
                "CHAIN-001",
                "执行",
                "经营事项",
                "合同收尾跟踪",
                "处理中",
                managerUserId,
                "跟踪尾款与验收",
                "REV-001",
                "CR-001")
        };
        var receivables = new List<BusinessReceivable>
        {
            new(
                Guid.Parse("40000000-0000-0000-0000-000000000001"),
                projectId,
                "REC-001",
                "首付款",
                new DateTime(2026, 3, 1),
                800_000m,
                200_000m,
                "待回款",
                "REV-001",
                "INV-001"),
            new(
                Guid.Parse("40000000-0000-0000-0000-000000000002"),
                otherProjectId,
                "REC-002",
                "尾款",
                new DateTime(2026, 5, 1),
                500_000m,
                500_000m,
                "已回款",
                "REV-002",
                "INV-002")
        };
        var invoices = new List<BusinessInvoice>
        {
            new(
                Guid.Parse("60000000-0000-0000-0000-000000000001"),
                projectId,
                "INV-001",
                "首付款开票",
                new DateTime(2026, 2, 20),
                800_000m,
                48_000m,
                "已开票",
                "REV-001",
                "REC-001"),
            new(
                Guid.Parse("60000000-0000-0000-0000-000000000002"),
                otherProjectId,
                "INV-002",
                "尾款开票",
                new DateTime(2026, 5, 10),
                500_000m,
                30_000m,
                "已开票",
                "REV-002",
                "REC-002")
        };
        var acceptances = new List<BusinessAcceptance>
        {
            new(
                Guid.Parse("70000000-0000-0000-0000-000000000001"),
                projectId,
                "ACC-001",
                "阶段验收",
                new DateTime(2026, 4, 10),
                "通过",
                "已验收",
                "REV-001",
                qualityInspectionCode: "QC-001"),
            new(
                Guid.Parse("70000000-0000-0000-0000-000000000002"),
                otherProjectId,
                "ACC-002",
                "最终验收",
                new DateTime(2026, 6, 10),
                "通过",
                "已验收",
                "REV-002",
                qualityInspectionCode: "QC-002")
        };
        var forecastHistories = new List<BusinessForecastHistory>
        {
            new(
                Guid.Parse("80000000-0000-0000-0000-000000000001"),
                projectId,
                "年末预计回款",
                "600000",
                "800000",
                managerUserId,
                "合同范围调整",
                "预测调整",
                "REV-001")
        };
        var performanceSnapshots = new List<ProjectPerformanceSnapshot>
        {
            new(
                Guid.Parse("90000000-0000-0000-0000-000000000001"),
                projectId,
                new DateTime(2026, 4, 30),
                "2026-W18",
                100m,
                80m,
                120m)
        };

        return new BusinessContext(
            Acceptances: acceptances,
            BudgetExecutions: budgetExecutions,
            BusinessChains: businessChains,
            Contracts: contracts,
            ForecastHistories: forecastHistories,
            Invoices: invoices,
            OrganizationUnitNameMap: new Dictionary<Guid, string>
            {
                [organizationUnitId] = "交付中心"
            },
            PerformanceSnapshots: performanceSnapshots,
            Procurements: procurements,
            ProjectCodeMap: new Dictionary<Guid, string>
            {
                [projectId] = "PRJ-001",
                [otherProjectId] = "PRJ-002"
            },
            ProjectNameMap: new Dictionary<Guid, string>
            {
                [projectId] = "智慧园区项目",
                [otherProjectId] = "数据中台项目"
            },
            Projects: [project, otherProject],
            Receivables: receivables,
            UserNameMap: new Dictionary<Guid, string>
            {
                [managerUserId] = "项目经理"
            });
    }
}
