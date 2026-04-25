namespace General.Admin.Project;

public class ProjectListInput
{
    public string? Keyword { get; set; }

    public bool OnlyMyRelated { get; set; }

    public string? Status { get; set; }
}
