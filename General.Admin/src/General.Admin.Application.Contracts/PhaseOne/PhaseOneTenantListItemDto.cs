namespace General.Admin.PhaseOne;

public class PhaseOneTenantListItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? DefaultConnectionString { get; set; }
}
