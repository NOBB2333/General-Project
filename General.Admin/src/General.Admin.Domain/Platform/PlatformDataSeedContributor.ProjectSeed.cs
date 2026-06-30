using Volo.Abp.Domain.Repositories;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor
{
    private async Task EnsureProjectSprintSeedDataAsync(
        IReadOnlyList<ProjectEntity> projects,
        Guid adminUserId,
        Guid pmoUserId,
        Guid pmUserId,
        Guid memberUserId)
    {
        var sprints = new[]
        {
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000003"), projects[1].Id, "SPR-001", "第 1 迭代 · 权限模型", adminUserId, new DateTime(2026, 4, 8), new DateTime(2026, 4, 19), "进行中", 58, "聚焦菜单鉴权、项目详情联调"),
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000006"), projects[0].Id, "SPR-001", "第 1 迭代 · 现场联调", pmUserId, new DateTime(2026, 4, 11), new DateTime(2026, 4, 25), "进行中", 55, "现场交付清单、联调和验收准备统一放入本次迭代。"),
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000007"), projects[0].Id, "SPR-002", "第 2 迭代 · 验收准备", memberUserId, new DateTime(2026, 4, 26), new DateTime(2026, 5, 8), "进行中", 45, "集中处理验收标准、检查项和现场阻塞。"),
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000008"), projects[1].Id, "SPR-002", "第 2 迭代 · 项目详情体验", pmUserId, new DateTime(2026, 4, 20), new DateTime(2026, 4, 28), "进行中", 40, "收敛项目详情页、文件上传和 Sprint 体验。"),
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000009"), projects[1].Id, "SPR-003", "第 3 迭代 · 表格交互", adminUserId, new DateTime(2026, 4, 29), new DateTime(2026, 5, 12), "未开始", 0, "集中处理项目详情交互和表格体验。"),
            new ProjectSprint(Guid.Parse("54000000-0000-0000-0000-000000000010"), projects[2].Id, "SPR-001", "第 1 迭代 · 财务接口联调", pmoUserId, new DateTime(2026, 4, 15), new DateTime(2026, 4, 30), "进行中", 35, "财务接口清单、回款节点和联调说明统一放入 Sprint。")
        };

        var existingSprints = await _projectSprintRepository.GetListAsync();
        var sprintLookup = existingSprints
            .ToDictionary(
                x => $"{x.ProjectId:N}:{x.SprintCode}",
                x => x,
                StringComparer.OrdinalIgnoreCase);
        var missingSprints = new List<ProjectSprint>();
        foreach (var desiredSprint in sprints)
        {
            if (sprintLookup.TryGetValue($"{desiredSprint.ProjectId:N}:{desiredSprint.SprintCode}", out var existingSprint))
            {
                existingSprint.Update(
                    desiredSprint.Name,
                    desiredSprint.OwnerUserId,
                    desiredSprint.StartDate,
                    desiredSprint.EndDate,
                    desiredSprint.Status,
                    desiredSprint.Progress,
                    desiredSprint.Summary);
                await _projectSprintRepository.UpdateAsync(existingSprint, autoSave: true);
                continue;
            }

            missingSprints.Add(desiredSprint);
        }

        if (missingSprints.Count > 0)
        {
            await _projectSprintRepository.InsertManyAsync(missingSprints, autoSave: true);
        }

        var refreshedSprintLookup = (await _projectSprintRepository.GetListAsync())
            .ToDictionary(
                x => $"{x.ProjectId:N}:{x.SprintCode}",
                x => x,
                StringComparer.OrdinalIgnoreCase);
        var taskSprintMap = new Dictionary<Guid, (Guid ProjectId, string SprintCode)>
        {
            [Guid.Parse("52000000-0000-0000-0000-000000000001")] = (projects[0].Id, "SPR-001"),
            [Guid.Parse("52000000-0000-0000-0000-000000000002")] = (projects[0].Id, "SPR-002"),
            [Guid.Parse("52000000-0000-0000-0000-000000000003")] = (projects[0].Id, "SPR-001"),
            [Guid.Parse("52000000-0000-0000-0000-000000000004")] = (projects[1].Id, "SPR-001"),
            [Guid.Parse("52000000-0000-0000-0000-000000000005")] = (projects[1].Id, "SPR-002"),
            [Guid.Parse("52000000-0000-0000-0000-000000000006")] = (projects[1].Id, "SPR-001"),
            [Guid.Parse("52000000-0000-0000-0000-000000000007")] = (projects[2].Id, "SPR-001"),
            [Guid.Parse("52000000-0000-0000-0000-000000000008")] = (projects[2].Id, "SPR-001")
        };

        foreach (var (taskId, sprintKey) in taskSprintMap)
        {
            var task = await _projectTaskRepository.FindAsync(taskId);
            if (task == null ||
                !refreshedSprintLookup.TryGetValue($"{sprintKey.ProjectId:N}:{sprintKey.SprintCode}", out var sprint) ||
                task.SprintId == sprint.Id)
            {
                continue;
            }

            task.UpdateWbs(
                task.ParentTaskId,
                sprint.Id,
                task.WbsCode,
                task.SortOrder,
                task.BaselineStartTime,
                task.BaselineEndTime,
                task.WbsNodeId,
                task.RequirementId);
            await _projectTaskRepository.UpdateAsync(task, autoSave: true);
        }
    }

    private async Task EnsureProjectMemberAsync(
        Guid id,
        Guid projectId,
        Guid userId,
        Guid organizationUnitId,
        IEnumerable<string> roleNames,
        DateTime joinDate)
    {
        var roles = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var exists = await _projectMemberRepository.AnyAsync(x =>
            x.ProjectId == projectId &&
            x.UserId == userId);
        if (exists)
        {
            await EnsureProjectMemberRolesAsync(id, projectId, userId, roles);
            return;
        }

        await _projectMemberRepository.InsertAsync(
            new ProjectMember(id, projectId, userId, organizationUnitId, roles, joinDate),
            autoSave: true);
        await EnsureProjectMemberRolesAsync(id, projectId, userId, roles);
    }

    private async Task EnsureProjectMemberRolesAsync(
        Guid projectMemberId,
        Guid projectId,
        Guid userId,
        IReadOnlyCollection<string> roleNames)
    {
        foreach (var roleName in roleNames)
        {
            var exists = await _projectMemberRoleRepository.AnyAsync(x =>
                x.ProjectMemberId == projectMemberId &&
                x.RoleName == roleName);
            if (exists)
            {
                continue;
            }

            await _projectMemberRoleRepository.InsertAsync(
                new ProjectMemberRole(Guid.NewGuid(), projectId, projectMemberId, userId, roleName),
                autoSave: true);
        }
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
