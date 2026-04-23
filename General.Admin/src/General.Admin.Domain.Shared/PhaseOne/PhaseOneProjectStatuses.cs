namespace General.Admin.PhaseOne;

/// <summary>
/// 项目/任务/里程碑/周期/文档等通用终态状态值常量。
/// </summary>
public static class PhaseOneProjectStatuses
{
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
public static class PhaseOneRaidTypes
{
    public const string Risk     = "风险";
    public const string Action   = "行动";
    public const string Issue    = "问题";
    public const string Decision = "决策";
}

/// <summary>
/// 优先级 / 级别常量（项目、任务、RAID、问题等通用）。
/// </summary>
public static class PhaseOnePriorities
{
    public const string High   = "高";
    public const string Medium = "中";
    public const string Low    = "低";
}

/// <summary>
/// 周期类型常量。
/// </summary>
public static class PhaseOneCycleTypes
{
    public const string Sprint  = "冲刺";
    public const string Regular = "常规";
}
