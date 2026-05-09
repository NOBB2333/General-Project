namespace General.Admin.Platform;

public class PlatformPagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];

    public int TotalCount { get; set; }
}
