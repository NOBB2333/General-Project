namespace General.Admin.Platform;

public static class PlatformWorkflowActions
{
    public const string Approve = "同意";
    public const string Reject = "驳回";
    public const string Submit = "提交";
    public const string Withdraw = "撤回";
}

public static class PlatformWorkflowDefinitionKeys
{
    public const string ProjectChangeApproval = "Project.Change.Approval";
    public const string ProjectCharterApproval = "Project.Charter.Approval";
}

public static class PlatformWorkflowStatuses
{
    public const string Approved = "已通过";
    public const string Pending = "审批中";
    public const string Rejected = "已驳回";
    public const string Withdrawn = "已撤回";

    public static bool IsActive(string status) => status == Pending;
}
