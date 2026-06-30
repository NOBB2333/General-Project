namespace General.Admin.Platform;

public class PlatformApprovalRecordDto
{
    public string Action { get; set; } = string.Empty;

    public Guid? ActorUserId { get; set; }

    public string ActorUserName { get; set; } = string.Empty;

    public string Comment { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string FromStatus { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string NodeName { get; set; } = string.Empty;

    public Guid ApprovalInstanceId { get; set; }

    public string ToStatus { get; set; } = string.Empty;
}

public class PlatformWorkflowInstanceDto
{
    public Guid? ApplicantUserId { get; set; }

    public string ApplicantUserName { get; set; } = string.Empty;

    public List<PlatformApprovalRecordDto> ApprovalRecords { get; set; } = [];

    public string BusinessCode { get; set; } = string.Empty;

    public string BusinessId { get; set; } = string.Empty;

    public string BusinessTitle { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public DateTime? CompletedTime { get; set; }

    public DateTime CreationTime { get; set; }

    public string CurrentNodeName { get; set; } = string.Empty;

    public string ElsaWorkflowInstanceId { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Status { get; set; } = string.Empty;

    public string WorkflowDefinitionKey { get; set; } = string.Empty;

    public int WorkflowDefinitionVersion { get; set; }
}
