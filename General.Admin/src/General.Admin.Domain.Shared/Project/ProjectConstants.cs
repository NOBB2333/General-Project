namespace General.Admin.Project;

/// <summary>
/// 项目/任务/里程碑/Sprint/文档等通用终态状态值常量。
/// </summary>
public static class ProjectStatuses
{
    public const string InProgress = "进行中";
    public const string Testing    = "测试中";
    public const string Accepting  = "验收中";
    public const string Completed = "已完成";
    public const string Closed    = "已关闭";
    public const string Released  = "已解除";

    /// <summary>
    /// 判断一个状态值是否属于"已终止"状态（不再需要跟进）。
    /// </summary>
    public static bool IsClosed(string status) =>
        status is Completed or Closed or Released;
}

/// <summary>
/// RAID 事项类型常量。
/// </summary>
public static class ProjectRaidTypes
{
    public const string Risk     = "风险";
    public const string Action   = "行动";
    public const string Issue    = "问题";
    public const string Decision = "决策";
}

/// <summary>
/// 优先级 / 级别常量（项目、任务、RAID、问题等通用）。
/// </summary>
public static class ProjectPriorities
{
    public const string P0 = "P0";
    public const string P1 = "P1";
    public const string P2 = "P2";
    public const string P3 = "P3";

    public static bool IsSupported(string? priority)
    {
        return priority?.Trim() is P0 or P1 or P2 or P3;
    }

    public static string Normalize(string? priority, string fallback = P1)
    {
        var value = priority?.Trim();
        return IsSupported(value) ? value! : fallback;
    }

    public static int ResolveWeight(string? priority)
    {
        return priority?.Trim() switch
        {
            P0 => 4,
            P1 => 3,
            P2 => 2,
            P3 => 1,
            _ => 0
        };
    }

    public static int ResolveRiskImpact(string? priority)
    {
        return priority?.Trim() switch
        {
            P0 => 5,
            P1 => 4,
            P2 => 3,
            P3 => 2,
            _ => 4
        };
    }
}

/// <summary>
/// Sprint 类型常量。
/// </summary>
public static class ProjectSprintTypes
{
    public const string Sprint = "Sprint";
}
