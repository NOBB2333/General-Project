namespace General.Admin.Project;

public class ProjectMemberItemDto
{
    public string AccessLevel { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public DateTime JoinDate { get; set; }

    public DateTime? LeaveDate { get; set; }

    public string OrganizationUnitName { get; set; } = string.Empty;

    public List<string> RoleNames { get; set; } = [];

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}
