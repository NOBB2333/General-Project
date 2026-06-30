using System;
using Shouldly;
using Xunit;

namespace General.Admin.Business;

public class BusinessEntityUpdateTests
{
    [Fact]
    public void BudgetExecution_Update_Should_Recalculate_Variance_When_Not_Provided()
    {
        var item = new BusinessBudgetExecution(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BGT-001",
            "实施成本",
            100_000m,
            30_000m,
            70_000m,
            1);

        item.Update(
            "BGT-002",
            "采购成本",
            120_000m,
            80_000m,
            varianceAmount: null,
            sortOrder: 2);

        item.BudgetCode.ShouldBe("BGT-002");
        item.Category.ShouldBe("采购成本");
        item.AdjustedAmount.ShouldBe(120_000m);
        item.ExecutedAmount.ShouldBe(80_000m);
        item.VarianceAmount.ShouldBe(40_000m);
        item.SortOrder.ShouldBe(2);
    }

    [Fact]
    public void Contract_Update_Should_Refresh_Core_Fields_And_Trim_Optional_References()
    {
        var projectId = Guid.NewGuid();
        var item = new BusinessContract(
            Guid.NewGuid(),
            projectId,
            "CON-001",
            "主合同",
            "主合同",
            "客户甲",
            100_000m,
            new DateTime(2026, 1, 1),
            "执行中",
            true);

        item.Update(
            "CON-002",
            "补充合同",
            "补充合同",
            "客户乙",
            160_000m,
            new DateTime(2026, 2, 1),
            "已签订",
            isRevenueContract: false,
            parentContractCode: " CON-001 ",
            sourceChangeCode: " CR-001 ");

        item.ProjectId.ShouldBe(projectId);
        item.ContractCode.ShouldBe("CON-002");
        item.Title.ShouldBe("补充合同");
        item.CounterpartyName.ShouldBe("客户乙");
        item.Amount.ShouldBe(160_000m);
        item.SignDate.ShouldBe(new DateTime(2026, 2, 1));
        item.Status.ShouldBe("已签订");
        item.IsRevenueContract.ShouldBeFalse();
        item.ParentContractCode.ShouldBe("CON-001");
        item.SourceChangeCode.ShouldBe("CR-001");
    }

    [Fact]
    public void Receivable_Update_Should_Refresh_Amount_And_Linked_Document()
    {
        var item = new BusinessReceivable(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "REC-001",
            "首付款",
            new DateTime(2026, 3, 1),
            200_000m,
            80_000m,
            "待回款");

        item.Update(
            "REC-002",
            "尾款",
            new DateTime(2026, 4, 1),
            300_000m,
            300_000m,
            "已回款",
            linkedContractCode: " CON-002 ",
            invoiceCode: " INV-002 ");

        item.ReceivableCode.ShouldBe("REC-002");
        item.Title.ShouldBe("尾款");
        item.PlannedDate.ShouldBe(new DateTime(2026, 4, 1));
        item.TotalAmount.ShouldBe(300_000m);
        item.ReceivedAmount.ShouldBe(300_000m);
        item.Status.ShouldBe("已回款");
        item.LinkedContractCode.ShouldBe("CON-002");
        item.InvoiceCode.ShouldBe("INV-002");
    }

    [Fact]
    public void Acceptance_Update_Should_Refresh_Status_And_Quality_Link()
    {
        var milestoneId = Guid.NewGuid();
        var item = new BusinessAcceptance(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ACC-001",
            "阶段验收",
            new DateTime(2026, 4, 1),
            "通过",
            "已验收");

        item.Update(
            "ACC-002",
            "最终验收",
            new DateTime(2026, 5, 1),
            "整改通过",
            "待归档",
            linkedContractCode: " CON-002 ",
            milestoneId: milestoneId,
            qualityInspectionCode: " QC-002 ");

        item.AcceptanceCode.ShouldBe("ACC-002");
        item.Title.ShouldBe("最终验收");
        item.AcceptanceDate.ShouldBe(new DateTime(2026, 5, 1));
        item.AcceptanceResult.ShouldBe("整改通过");
        item.Status.ShouldBe("待归档");
        item.LinkedContractCode.ShouldBe("CON-002");
        item.MilestoneId.ShouldBe(milestoneId);
        item.QualityInspectionCode.ShouldBe("QC-002");
    }
}
