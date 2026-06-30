using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Project;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectQueryServiceResourceVisibilityTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private static readonly Guid OtherUserId = Guid.Parse("5b5e9a2e-7b0c-4c83-94de-09a1e8f4cc11");
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectMemberRole, Guid> _projectMemberRoleRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly ProjectQueryService _projectQueryService;
    private readonly IRepository<ProjectTaskDependency, Guid> _projectTaskDependencyRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectQueryServiceResourceVisibilityTests()
    {
        _projectMemberRepository = GetRequiredService<IRepository<ProjectMember, Guid>>();
        _projectMemberRoleRepository = GetRequiredService<IRepository<ProjectMemberRole, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectQueryService = GetRequiredService<ProjectQueryService>();
        _projectTaskDependencyRepository = GetRequiredService<IRepository<ProjectTaskDependency, Guid>>();
        _projectTaskRepository = GetRequiredService<IRepository<ProjectTask, Guid>>();
        _projectWorklogRepository = GetRequiredService<IRepository<ProjectWorklog, Guid>>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task GetDetailAsync_Should_Show_Only_Own_Resource_View_For_Ordinary_Member()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateFixtureAsync(OtherUserId, OtherUserId, currentUserRoles: ["开发"]);

            var detail = await _projectQueryService.GetDetailAsync(fixture.ProjectId);

            detail.Members.Select(item => item.UserId).ToArray().ShouldBe([CurrentUserId]);
            detail.Tasks.Select(item => item.Id).ToArray().ShouldBe([fixture.CurrentUserTaskId]);
            detail.Worklogs.Select(item => item.UserId).Distinct().ToArray().ShouldBe([CurrentUserId]);
            detail.TotalWorklogHours.ShouldBe(3.5);
            detail.TaskDependencies.ShouldBeEmpty();
            detail.MemberCount.ShouldBe(1);
        });
    }

    [Fact]
    public async Task GetDetailAsync_Should_Show_Team_Resource_View_For_Project_Manager()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var fixture = await CreateFixtureAsync(CurrentUserId, OtherUserId, currentUserRoles: ["PM"]);

            var detail = await _projectQueryService.GetDetailAsync(fixture.ProjectId);

            detail.Members.Select(item => item.UserId).OrderBy(item => item).ToArray()
                .ShouldBe(new[] { CurrentUserId, OtherUserId }.OrderBy(item => item).ToArray());
            detail.Tasks.Select(item => item.Id).OrderBy(item => item).ToArray()
                .ShouldBe(new[] { fixture.CurrentUserTaskId, fixture.OtherUserTaskId }.OrderBy(item => item).ToArray());
            detail.Worklogs.Select(item => item.UserId).Distinct().OrderBy(item => item).ToArray()
                .ShouldBe(new[] { CurrentUserId, OtherUserId }.OrderBy(item => item).ToArray());
            detail.TotalWorklogHours.ShouldBe(10.5);
            detail.TaskDependencies.Count.ShouldBe(1);
            detail.MemberCount.ShouldBe(2);
        });
    }

    private async Task<ResourceVisibilityFixture> CreateFixtureAsync(
        Guid managerUserId,
        Guid otherUserId,
        string[] currentUserRoles)
    {
        await EnsureUserAsync(CurrentUserId, "resource.current");
        await EnsureUserAsync(otherUserId, "resource.other");

        var projectId = Guid.NewGuid();
        var organizationUnitId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                NewCode("PRJ"),
                "资源可见性测试项目",
                null,
                "软件项目",
                "内部立项",
                organizationUnitId,
                managerUserId,
                otherUserId,
                ProjectPriorities.P1,
                ProjectStatuses.InProgress,
                DateTime.Today,
                DateTime.Today.AddDays(30),
                false,
                "用于验证人员与资源的普通成员和 PM 可见性。"),
            autoSave: true);

        var currentMemberId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid();
        await _projectMemberRepository.InsertManyAsync(
        [
            new ProjectMember(currentMemberId, projectId, CurrentUserId, organizationUnitId, currentUserRoles, DateTime.Today),
            new ProjectMember(otherMemberId, projectId, otherUserId, organizationUnitId, ["测试"], DateTime.Today)
        ], autoSave: true);
        await _projectMemberRoleRepository.InsertManyAsync(
            currentUserRoles
                .Select(roleName => new ProjectMemberRole(Guid.NewGuid(), projectId, currentMemberId, CurrentUserId, roleName))
                .Append(new ProjectMemberRole(Guid.NewGuid(), projectId, otherMemberId, otherUserId, "测试"))
                .ToList(),
            autoSave: true);

        var currentUserTaskId = Guid.NewGuid();
        var otherUserTaskId = Guid.NewGuid();
        await _projectTaskRepository.InsertManyAsync(
        [
            new ProjectTask(
                currentUserTaskId,
                projectId,
                organizationUnitId,
                NewCode("TASK"),
                "当前成员任务",
                CurrentUserId,
                ProjectStatuses.InProgress,
                ProjectPriorities.P1,
                DateTime.Today,
                DateTime.Today.AddDays(3),
                estimatedWorkHours: 8,
                developerOwnerUserId: CurrentUserId),
            new ProjectTask(
                otherUserTaskId,
                projectId,
                organizationUnitId,
                NewCode("TASK"),
                "其他成员任务",
                otherUserId,
                ProjectStatuses.InProgress,
                ProjectPriorities.P1,
                DateTime.Today,
                DateTime.Today.AddDays(5),
                estimatedWorkHours: 6,
                testerOwnerUserId: otherUserId)
        ], autoSave: true);

        await _projectWorklogRepository.InsertManyAsync(
        [
            new ProjectWorklog(
                Guid.NewGuid(),
                projectId,
                CurrentUserId,
                GetWeekStart(DateTime.Today),
                DateTime.Today,
                3.5,
                "当前成员工时。",
                currentUserTaskId),
            new ProjectWorklog(
                Guid.NewGuid(),
                projectId,
                otherUserId,
                GetWeekStart(DateTime.Today),
                DateTime.Today,
                7,
                "其他成员工时。",
                otherUserTaskId)
        ], autoSave: true);
        await _projectTaskDependencyRepository.InsertAsync(
            new ProjectTaskDependency(
                Guid.NewGuid(),
                projectId,
                currentUserTaskId,
                otherUserTaskId,
                "FinishToStart"),
            autoSave: true);

        return new ResourceVisibilityFixture(projectId, currentUserTaskId, otherUserTaskId);
    }

    private async Task EnsureUserAsync(Guid userId, string userName)
    {
        if (await _userRepository.AnyAsync(item => item.Id == userId))
        {
            return;
        }

        var user = new IdentityUser(userId, userName, $"{userName}@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private sealed record ResourceVisibilityFixture(Guid ProjectId, Guid CurrentUserTaskId, Guid OtherUserTaskId);
}
