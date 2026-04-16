namespace General.Admin.PhaseOne;

public class PhaseOneProjectTaskListInput
{
    public string? Keyword { get; set; }

    public bool OnlyMine { get; set; }

    public Guid? ProjectId { get; set; }

    public string? Status { get; set; }
}
