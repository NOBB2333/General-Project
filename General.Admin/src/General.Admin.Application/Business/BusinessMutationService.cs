using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

[UnitOfWork(true)]
public class BusinessMutationService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<BusinessAcceptance, Guid> _acceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessInvoice, Guid> _invoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public BusinessMutationService(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<BusinessAcceptance, Guid> acceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> budgetExecutionRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessInvoice, Guid> invoiceRepository,
        IRepository<BusinessProcurement, Guid> procurementRepository,
        IRepository<BusinessReceivable, Guid> receivableRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _acceptanceRepository = acceptanceRepository;
        _budgetExecutionRepository = budgetExecutionRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _invoiceRepository = invoiceRepository;
        _procurementRepository = procurementRepository;
        _receivableRepository = receivableRepository;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }

    public virtual async Task<Guid> CreateBudgetExecutionAsync(Guid projectId, BusinessBudgetExecutionSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _budgetExecutionRepository.AnyAsync(item => item.BudgetCode == input.BudgetCode))
        {
            throw new BusinessException("预算编号已存在。");
        }

        var item = new BusinessBudgetExecution(
            Guid.NewGuid(),
            projectId,
            input.BudgetCode,
            input.Category,
            input.AdjustedAmount,
            input.ExecutedAmount,
            input.VarianceAmount ?? input.AdjustedAmount - input.ExecutedAmount,
            input.SortOrder);
        await _budgetExecutionRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateAcceptanceAsync(Guid projectId, BusinessAcceptanceSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _acceptanceRepository.AnyAsync(item => item.AcceptanceCode == input.AcceptanceCode))
        {
            throw new BusinessException("验收编号已存在。");
        }

        var item = new BusinessAcceptance(
            Guid.NewGuid(),
            projectId,
            input.AcceptanceCode,
            input.Title,
            input.AcceptanceDate ?? DateTime.Today,
            input.AcceptanceResult,
            input.Status,
            input.LinkedContractCode,
            input.MilestoneId,
            input.QualityInspectionCode);
        await _acceptanceRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateBusinessChainAsync(Guid projectId, BusinessChainSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        await EnsureUserExistsAsync(input.OwnerUserId, "经营链负责人不存在。");
        if (await _businessChainRepository.AnyAsync(item => item.ChainCode == input.ChainCode))
        {
            throw new BusinessException("经营链编号已存在。");
        }

        var item = new BusinessChain(
            Guid.NewGuid(),
            projectId,
            input.ChainCode,
            input.Stage,
            input.Type,
            input.Title,
            input.Status,
            input.OwnerUserId,
            input.Summary,
            input.LinkedContractCode,
            input.SourceChangeCode);
        await _businessChainRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateContractAsync(Guid projectId, BusinessContractSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _businessContractRepository.AnyAsync(item => item.ContractCode == input.ContractCode))
        {
            throw new BusinessException("合同编号已存在。");
        }

        var item = new BusinessContract(
            Guid.NewGuid(),
            projectId,
            input.ContractCode,
            input.Title,
            input.Type,
            input.CounterpartyName,
            input.Amount,
            input.SignDate ?? DateTime.Today,
            input.Status,
            input.IsRevenueContract,
            input.ParentContractCode,
            input.SourceChangeCode);
        await _businessContractRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateProcurementAsync(Guid projectId, BusinessProcurementSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _procurementRepository.AnyAsync(item => item.ProcurementCode == input.ProcurementCode))
        {
            throw new BusinessException("采购编号已存在。");
        }

        var item = new BusinessProcurement(
            Guid.NewGuid(),
            projectId,
            input.ProcurementCode,
            input.Title,
            input.SupplierName,
            input.Amount,
            input.SignDate ?? DateTime.Today,
            input.Stage,
            input.Status,
            input.LinkedContractCode,
            input.SourceChangeCode);
        await _procurementRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateInvoiceAsync(Guid projectId, BusinessInvoiceSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _invoiceRepository.AnyAsync(item => item.InvoiceCode == input.InvoiceCode))
        {
            throw new BusinessException("发票编号已存在。");
        }

        var item = new BusinessInvoice(
            Guid.NewGuid(),
            projectId,
            input.InvoiceCode,
            input.Title,
            input.InvoiceDate ?? DateTime.Today,
            input.Amount,
            input.TaxAmount,
            input.Status,
            input.LinkedContractCode,
            input.ReceivableCode);
        await _invoiceRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task<Guid> CreateReceivableAsync(Guid projectId, BusinessReceivableSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        if (await _receivableRepository.AnyAsync(item => item.ReceivableCode == input.ReceivableCode))
        {
            throw new BusinessException("回款编号已存在。");
        }

        var item = new BusinessReceivable(
            Guid.NewGuid(),
            projectId,
            input.ReceivableCode,
            input.Title,
            input.PlannedDate ?? DateTime.Today,
            input.TotalAmount,
            input.ReceivedAmount,
            input.Status,
            input.LinkedContractCode,
            input.InvoiceCode);
        await _receivableRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public virtual async Task UpdateBudgetExecutionAsync(Guid projectId, Guid budgetExecutionId, BusinessBudgetExecutionSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_budgetExecutionRepository, projectId, budgetExecutionId, "预算执行项不存在或不属于当前项目。");
        await EnsureUniqueAsync(_budgetExecutionRepository, budgetExecutionId, entity => entity.BudgetCode == input.BudgetCode, "预算编号已存在。");

        item.Update(
            input.BudgetCode,
            input.Category,
            input.AdjustedAmount,
            input.ExecutedAmount,
            input.VarianceAmount,
            input.SortOrder);
        await _budgetExecutionRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateAcceptanceAsync(Guid projectId, Guid acceptanceId, BusinessAcceptanceSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_acceptanceRepository, projectId, acceptanceId, "验收记录不存在或不属于当前项目。");
        await EnsureUniqueAsync(_acceptanceRepository, acceptanceId, entity => entity.AcceptanceCode == input.AcceptanceCode, "验收编号已存在。");

        item.Update(
            input.AcceptanceCode,
            input.Title,
            input.AcceptanceDate ?? DateTime.Today,
            input.AcceptanceResult,
            input.Status,
            input.LinkedContractCode,
            input.MilestoneId,
            input.QualityInspectionCode);
        await _acceptanceRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateBusinessChainAsync(Guid projectId, Guid chainId, BusinessChainSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        await EnsureUserExistsAsync(input.OwnerUserId, "经营链负责人不存在。");
        var item = await GetProjectOwnedAsync(_businessChainRepository, projectId, chainId, "经营链事项不存在或不属于当前项目。");
        await EnsureUniqueAsync(_businessChainRepository, chainId, entity => entity.ChainCode == input.ChainCode, "经营链编号已存在。");

        item.Update(
            input.ChainCode,
            input.Stage,
            input.Type,
            input.Title,
            input.Status,
            input.OwnerUserId,
            input.Summary,
            input.LinkedContractCode,
            input.SourceChangeCode);
        await _businessChainRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateContractAsync(Guid projectId, Guid contractId, BusinessContractSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_businessContractRepository, projectId, contractId, "合同不存在或不属于当前项目。");
        await EnsureUniqueAsync(_businessContractRepository, contractId, entity => entity.ContractCode == input.ContractCode, "合同编号已存在。");

        item.Update(
            input.ContractCode,
            input.Title,
            input.Type,
            input.CounterpartyName,
            input.Amount,
            input.SignDate ?? DateTime.Today,
            input.Status,
            input.IsRevenueContract,
            input.ParentContractCode,
            input.SourceChangeCode);
        await _businessContractRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateProcurementAsync(Guid projectId, Guid procurementId, BusinessProcurementSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_procurementRepository, projectId, procurementId, "采购事项不存在或不属于当前项目。");
        await EnsureUniqueAsync(_procurementRepository, procurementId, entity => entity.ProcurementCode == input.ProcurementCode, "采购编号已存在。");

        item.Update(
            input.ProcurementCode,
            input.Title,
            input.SupplierName,
            input.Amount,
            input.SignDate ?? DateTime.Today,
            input.Stage,
            input.Status,
            input.LinkedContractCode,
            input.SourceChangeCode);
        await _procurementRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateInvoiceAsync(Guid projectId, Guid invoiceId, BusinessInvoiceSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_invoiceRepository, projectId, invoiceId, "发票记录不存在或不属于当前项目。");
        await EnsureUniqueAsync(_invoiceRepository, invoiceId, entity => entity.InvoiceCode == input.InvoiceCode, "发票编号已存在。");

        item.Update(
            input.InvoiceCode,
            input.Title,
            input.InvoiceDate ?? DateTime.Today,
            input.Amount,
            input.TaxAmount,
            input.Status,
            input.LinkedContractCode,
            input.ReceivableCode);
        await _invoiceRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task UpdateReceivableAsync(Guid projectId, Guid receivableId, BusinessReceivableSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(_receivableRepository, projectId, receivableId, "回款记录不存在或不属于当前项目。");
        await EnsureUniqueAsync(_receivableRepository, receivableId, entity => entity.ReceivableCode == input.ReceivableCode, "回款编号已存在。");

        item.Update(
            input.ReceivableCode,
            input.Title,
            input.PlannedDate ?? DateTime.Today,
            input.TotalAmount,
            input.ReceivedAmount,
            input.Status,
            input.LinkedContractCode,
            input.InvoiceCode);
        await _receivableRepository.UpdateAsync(item, autoSave: true);
    }

    public virtual async Task DeleteBudgetExecutionAsync(Guid projectId, Guid budgetExecutionId)
    {
        await DeleteProjectOwnedAsync(_budgetExecutionRepository, projectId, budgetExecutionId, "预算执行项不存在或不属于当前项目。");
    }

    public virtual async Task DeleteAcceptanceAsync(Guid projectId, Guid acceptanceId)
    {
        await DeleteProjectOwnedAsync(_acceptanceRepository, projectId, acceptanceId, "验收记录不存在或不属于当前项目。");
    }

    public virtual async Task DeleteBusinessChainAsync(Guid projectId, Guid chainId)
    {
        await DeleteProjectOwnedAsync(_businessChainRepository, projectId, chainId, "经营链事项不存在或不属于当前项目。");
    }

    public virtual async Task DeleteContractAsync(Guid projectId, Guid contractId)
    {
        await DeleteProjectOwnedAsync(_businessContractRepository, projectId, contractId, "合同不存在或不属于当前项目。");
    }

    public virtual async Task DeleteProcurementAsync(Guid projectId, Guid procurementId)
    {
        await DeleteProjectOwnedAsync(_procurementRepository, projectId, procurementId, "采购事项不存在或不属于当前项目。");
    }

    public virtual async Task DeleteInvoiceAsync(Guid projectId, Guid invoiceId)
    {
        await DeleteProjectOwnedAsync(_invoiceRepository, projectId, invoiceId, "发票记录不存在或不属于当前项目。");
    }

    public virtual async Task DeleteReceivableAsync(Guid projectId, Guid receivableId)
    {
        await DeleteProjectOwnedAsync(_receivableRepository, projectId, receivableId, "回款记录不存在或不属于当前项目。");
    }

    private async Task EnsureProjectAccessAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        if (_currentUser.IsInRole(PlatformRoleNames.Admin) || _currentUser.IsInRole(PlatformRoleNames.Pmo))
        {
            return;
        }

        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        if (accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId))
        {
            return;
        }

        if (_currentUser.Id.HasValue)
        {
            var currentUserId = _currentUser.Id.Value;
            if (project.ManagerUserId == currentUserId || project.SponsorUserId == currentUserId)
            {
                return;
            }

            if (await _projectMemberRepository.AnyAsync(member =>
                member.ProjectId == projectId
                && member.UserId == currentUserId
                && (!member.LeaveDate.HasValue || member.LeaveDate.Value.Date >= DateTime.Today || member.AllowHistoricalRead)))
            {
                return;
            }
        }

        throw new AbpAuthorizationException("项目不存在或当前用户无权维护经营信息。");
    }

    private async Task DeleteProjectOwnedAsync<TEntity>(
        IRepository<TEntity, Guid> repository,
        Guid projectId,
        Guid itemId,
        string notFoundMessage)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        await EnsureProjectAccessAsync(projectId);
        var item = await GetProjectOwnedAsync(repository, projectId, itemId, notFoundMessage);
        await repository.DeleteAsync(item, autoSave: true);
    }

    private async Task<TEntity> GetProjectOwnedAsync<TEntity>(
        IRepository<TEntity, Guid> repository,
        Guid projectId,
        Guid itemId,
        string notFoundMessage)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        var item = await repository.GetAsync(itemId);
        var itemProjectId = ResolveProjectId(item);
        if (itemProjectId != projectId)
        {
            throw new BusinessException(notFoundMessage);
        }

        return item;
    }

    private static Guid ResolveProjectId<TEntity>(TEntity item)
    {
        return item switch
        {
            BusinessAcceptance acceptance => acceptance.ProjectId,
            BusinessBudgetExecution budgetExecution => budgetExecution.ProjectId,
            BusinessChain businessChain => businessChain.ProjectId,
            BusinessContract businessContract => businessContract.ProjectId,
            BusinessInvoice invoice => invoice.ProjectId,
            BusinessProcurement procurement => procurement.ProjectId,
            BusinessReceivable receivable => receivable.ProjectId,
            _ => throw new BusinessException("经营记录类型不支持项目归属校验。")
        };
    }

    private static async Task EnsureUniqueAsync<TEntity>(
        IRepository<TEntity, Guid> repository,
        Guid currentId,
        System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate,
        string message)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        var duplicate = await repository.FirstOrDefaultAsync(predicate);
        if (duplicate != null && !duplicate.Id.Equals(currentId))
        {
            throw new BusinessException(message);
        }
    }

    private async Task EnsureUserExistsAsync(Guid userId, string message)
    {
        if (userId == Guid.Empty || !await _userRepository.AnyAsync(item => item.Id == userId))
        {
            throw new BusinessException(message);
        }
    }
}
