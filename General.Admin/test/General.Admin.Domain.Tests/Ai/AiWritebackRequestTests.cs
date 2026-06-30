using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace General.Admin.Ai;

public class AiWritebackRequestTests
{
    [Fact]
    public void Approve_Should_Record_Approver_And_Block_Reprocessing()
    {
        var request = CreateRequest();
        var approverUserId = Guid.NewGuid();

        request.Approve(approverUserId);

        request.Status.ShouldBe("Approved");
        request.ApproverUserId.ShouldBe(approverUserId);
        request.ApprovedTime.ShouldNotBeNull();
        Should.Throw<BusinessException>(() => request.Reject("重复处理", approverUserId));
    }

    [Fact]
    public void Reject_Should_Record_Reason_And_Block_Reprocessing()
    {
        var request = CreateRequest();
        var approverUserId = Guid.NewGuid();

        request.Reject("缺少业务依据", approverUserId);

        request.Status.ShouldBe("Rejected");
        request.ApproverUserId.ShouldBe(approverUserId);
        request.RejectReason.ShouldBe("缺少业务依据");
        request.ApprovedTime.ShouldNotBeNull();
        Should.Throw<BusinessException>(() => request.Approve(approverUserId));
    }

    private static AiWritebackRequest CreateRequest()
    {
        return new AiWritebackRequest(
            Guid.NewGuid(),
            "Project",
            "CreateRisk",
            "ProjectRaidItem",
            "risk-001",
            "根据会议纪要新增项目风险",
            "{\"riskTitle\":\"客户验收延期\"}",
            "{\"after\":{\"status\":\"待确认\"}}",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}
