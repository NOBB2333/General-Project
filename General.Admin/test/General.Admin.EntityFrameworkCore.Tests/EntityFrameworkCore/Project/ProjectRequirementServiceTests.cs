using System;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Project;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectRequirementServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly ProjectRequirementService _service;
    private readonly IRepository<ProjectWbsNode, Guid> _projectWbsNodeRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectRequirementServiceTests()
    {
        _projectMilestoneRepository = GetRequiredService<IRepository<ProjectMilestone, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _projectRequirementRepository = GetRequiredService<IRepository<ProjectRequirement, Guid>>();
        _projectWbsNodeRepository = GetRequiredService<IRepository<ProjectWbsNode, Guid>>();
        _service = GetRequiredService<ProjectRequirementService>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task CreateRequirementAsync_Should_Save_Requirement_With_Wbs_And_Milestone()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求创建测试项目");
            var wbsNodeId = await CreateWbsNodeAsync(projectId, "1.1", "评分规则工作包");
            var milestoneId = await CreateMilestoneAsync(projectId, "评分规则验收");

            var requirementId = await _service.CreateRequirementAsync(
                projectId,
                new ProjectRequirementSaveInput
                {
                    AcceptanceCriteria = "管理员可以配置并保存评分规则。",
                    BusinessOwnerUserId = CurrentUserId,
                    Description = "支持按项目维护评分规则。",
                    MilestoneId = milestoneId,
                    Priority = ProjectPriorities.P0,
                    ProductOwnerUserId = CurrentUserId,
                    RequirementCode = "REQ-001",
                    SizeEstimate = 8,
                    Source = "客户",
                    Status = "评审中",
                    Title = "评分规则配置",
                    Type = "功能",
                    VersionNo = 1,
                    WbsNodeId = wbsNodeId
                });

            var requirement = await _projectRequirementRepository.GetAsync(requirementId);
            requirement.ProjectId.ShouldBe(projectId);
            requirement.RequirementCode.ShouldBe("REQ-001");
            requirement.WbsNodeId.ShouldBe(wbsNodeId);
            requirement.MilestoneId.ShouldBe(milestoneId);
            requirement.BusinessOwnerUserId.ShouldBe(CurrentUserId);
            requirement.ProductOwnerUserId.ShouldBe(CurrentUserId);
            requirement.SizeEstimate.ShouldBe(8);
        });
    }

    [Fact]
    public async Task CreateRequirementAsync_Should_Reject_Wbs_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求 WBS 归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他需求 WBS 项目");
            var otherWbsNodeId = await CreateWbsNodeAsync(otherProjectId, "9.1", "其他项目工作包");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateRequirementAsync(projectId, CreateRequirementInput(wbsNodeId: otherWbsNodeId)));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("WBS 节点必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateRequirementAsync_Should_Reject_Milestone_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求里程碑归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他需求里程碑项目");
            var otherMilestoneId = await CreateMilestoneAsync(otherProjectId, "其他项目里程碑");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateRequirementAsync(projectId, CreateRequirementInput(milestoneId: otherMilestoneId)));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("里程碑必须属于当前项目");
        });
    }

    [Fact]
    public async Task CreateRequirementAsync_Should_Reject_Replaced_Requirement_From_Other_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateProjectAsync("需求替代归属测试项目");
            var otherProjectId = await CreateProjectAsync("其他需求替代项目");
            var otherRequirementId = await _service.CreateRequirementAsync(otherProjectId, CreateRequirementInput("REQ-OLD"));

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.CreateRequirementAsync(projectId, CreateRequirementInput("REQ-NEW", replacesRequirementId: otherRequirementId)));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("被替代需求必须属于当前项目");
        });
    }

    private async Task<Guid> CreateProjectAsync(string name)
    {
        await EnsureCurrentUserAsync();
        var projectId = Guid.NewGuid();
        await _projectRepository.InsertAsync(
            new ProjectEntity(
                projectId,
                NewCode("PRJ"),
                name,
                null,
                "软件项目",
                "测试客户",
                Guid.NewGuid(),
                CurrentUserId,
                CurrentUserId,
                ProjectPriorities.P1,
                ProjectStatuses.InProgress,
                DateTime.Today,
                DateTime.Today.AddDays(30),
                false,
                "用于验证需求模型。"),
            autoSave: true);
        return projectId;
    }

    private ProjectRequirementSaveInput CreateRequirementInput(
        string requirementCode = "REQ-001",
        Guid? wbsNodeId = null,
        Guid? milestoneId = null,
        Guid? replacesRequirementId = null)
    {
        return new ProjectRequirementSaveInput
        {
            AcceptanceCriteria = "可以完成基础验收。",
            Description = "验证需求保存。",
            MilestoneId = milestoneId,
            Priority = ProjectPriorities.P1,
            ReplacesRequirementId = replacesRequirementId,
            RequirementCode = requirementCode,
            Source = "内部",
            Status = "草稿",
            Title = $"测试需求 {requirementCode}",
            Type = "功能",
            WbsNodeId = wbsNodeId
        };
    }

    private async Task<Guid> CreateMilestoneAsync(Guid projectId, string name)
    {
        var milestone = new ProjectMilestone(
            Guid.NewGuid(),
            projectId,
            name,
            CurrentUserId,
            DateTime.Today.AddDays(10),
            ProjectStatuses.InProgress);
        await _projectMilestoneRepository.InsertAsync(milestone, autoSave: true);
        return milestone.Id;
    }

    private async Task<Guid> CreateWbsNodeAsync(Guid projectId, string wbsCode, string name)
    {
        var node = new ProjectWbsNode(
            Guid.NewGuid(),
            projectId,
            wbsCode,
            name,
            "工作包",
            "规划中");
        await _projectWbsNodeRepository.InsertAsync(node, autoSave: true);
        return node.Id;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "project.requirement.tester", "project.requirement.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
