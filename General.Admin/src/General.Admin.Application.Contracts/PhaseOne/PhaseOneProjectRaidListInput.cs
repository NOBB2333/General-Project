namespace General.Admin.PhaseOne;

public class PhaseOneProjectRaidListInput
{
    public string? Keyword { get; set; }

    public bool OnlyOpen { get; set; } = true;

    public string? Type { get; set; }
}
