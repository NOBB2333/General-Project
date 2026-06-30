using System;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Project;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class BusinessMutationServiceTests : AdminEntityFrameworkCoreTestBase
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly BusinessMutationService _service;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public BusinessMutationServiceTests()
    {
        _budgetRepository = GetRequiredService<IRepository<BusinessBudgetExecution, Guid>>();
        _receivableRepository = GetRequiredService<IRepository<BusinessReceivable, Guid>>();
        _service = GetRequiredService<BusinessMutationService>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _userRepository = GetRequiredService<IRepository<IdentityUser, Guid>>();
    }

    [Fact]
    public async Task UpdateBudgetExecutionAsync_Should_Update_Project_Owned_Item_And_Recalculate_Variance()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateAccessibleProjectAsync();
            var itemId = await _service.CreateBudgetExecutionAsync(
                projectId,
                new BusinessBudgetExecutionSaveInput
                {
                    AdjustedAmount = 1200m,
                    BudgetCode = NewCode("BUD"),
                    Category = "实施成本",
                    ExecutedAmount = 200m,
                    SortOrder = 1
                });

            var nextCode = NewCode("BUD");
            await _service.UpdateBudgetExecutionAsync(
                projectId,
                itemId,
                new BusinessBudgetExecutionSaveInput
                {
                    AdjustedAmount = 1600m,
                    BudgetCode = nextCode,
                    Category = "交付成本",
                    ExecutedAmount = 500m,
                    SortOrder = 3
                });

            var item = await _budgetRepository.GetAsync(itemId);
            item.BudgetCode.ShouldBe(nextCode);
            item.Category.ShouldBe("交付成本");
            item.SortOrder.ShouldBe(3);
            item.VarianceAmount.ShouldBe(1100m);
        });
    }

    [Fact]
    public async Task UpdateBudgetExecutionAsync_Should_Reject_Duplicate_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateAccessibleProjectAsync();
            var duplicateCode = NewCode("BUD");
            await _service.CreateBudgetExecutionAsync(
                projectId,
                new BusinessBudgetExecutionSaveInput
                {
                    AdjustedAmount = 100m,
                    BudgetCode = duplicateCode,
                    Category = "原始预算",
                    ExecutedAmount = 20m,
                    SortOrder = 1
                });
            var itemId = await _service.CreateBudgetExecutionAsync(
                projectId,
                new BusinessBudgetExecutionSaveInput
                {
                    AdjustedAmount = 200m,
                    BudgetCode = NewCode("BUD"),
                    Category = "待调整预算",
                    ExecutedAmount = 30m,
                    SortOrder = 2
                });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.UpdateBudgetExecutionAsync(
                    projectId,
                    itemId,
                    new BusinessBudgetExecutionSaveInput
                    {
                        AdjustedAmount = 200m,
                        BudgetCode = duplicateCode,
                        Category = "待调整预算",
                        ExecutedAmount = 30m,
                        SortOrder = 2
                    }));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("预算编号已存在");
        });
    }

    [Fact]
    public async Task DeleteReceivableAsync_Should_Delete_Project_Owned_Item()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var projectId = await CreateAccessibleProjectAsync();
            var itemId = await _service.CreateReceivableAsync(
                projectId,
                new BusinessReceivableSaveInput
                {
                    InvoiceCode = "INV-DEL",
                    LinkedContractCode = "CON-DEL",
                    PlannedDate = DateTime.Today,
                    ReceivableCode = NewCode("REC"),
                    ReceivedAmount = 100m,
                    Status = "待回款",
                    Title = "删除验证回款",
                    TotalAmount = 300m
                });

            await _service.DeleteReceivableAsync(projectId, itemId);

            (await _receivableRepository.FindAsync(itemId)).ShouldBeNull();
        });
    }

    [Fact]
    public async Task DeleteReceivableAsync_Should_Reject_Item_From_Another_Project()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var sourceProjectId = await CreateAccessibleProjectAsync();
            var anotherProjectId = await CreateAccessibleProjectAsync();
            var itemId = await _service.CreateReceivableAsync(
                sourceProjectId,
                new BusinessReceivableSaveInput
                {
                    InvoiceCode = "INV-OTHER",
                    LinkedContractCode = "CON-OTHER",
                    PlannedDate = DateTime.Today,
                    ReceivableCode = NewCode("REC"),
                    ReceivedAmount = 100m,
                    Status = "待回款",
                    Title = "跨项目删除验证回款",
                    TotalAmount = 300m
                });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _service.DeleteReceivableAsync(anotherProjectId, itemId));

            exception.Code.ShouldNotBeNull();
            exception.Code.ShouldContain("不属于当前项目");
            (await _receivableRepository.FindAsync(itemId)).ShouldNotBeNull();
        });
    }

    private async Task<Guid> CreateAccessibleProjectAsync()
    {
        await EnsureCurrentUserAsync();
        var projectId = Guid.NewGuid();
        var project = new ProjectEntity(
            projectId,
            NewCode("PRJ"),
            "经营 CRUD 集成测试项目",
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
            "用于验证经营写侧更新和删除");
        await _projectRepository.InsertAsync(project, autoSave: true);
        return projectId;
    }

    private async Task EnsureCurrentUserAsync()
    {
        if (await _userRepository.AnyAsync(item => item.Id == CurrentUserId))
        {
            return;
        }

        var user = new IdentityUser(CurrentUserId, "business.mutation.tester", "business.mutation.tester@general.local");
        await _userRepository.InsertAsync(user, autoSave: true);
    }

    private static string NewCode(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}"[..32];
    }
}
