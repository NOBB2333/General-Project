namespace General.Admin.Platform;

public class PlatformApprovalActionInput
{
    public string Comment { get; set; } = string.Empty;
}

public class PlatformWorkflowStartInput
{
    public Guid? ApplicantUserId { get; set; }

    public string BusinessCode { get; set; } = string.Empty;

    public string BusinessId { get; set; } = string.Empty;

    public string BusinessSnapshot { get; set; } = string.Empty;

    public string BusinessTitle { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string WorkflowDefinitionKey { get; set; } = string.Empty;

    public int WorkflowDefinitionVersion { get; set; } = 1;
}
