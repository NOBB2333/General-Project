using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using General.Admin.Project;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

public class BusinessContextLoader : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<BusinessAcceptance, Guid> _acceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessForecastHistory, Guid> _forecastHistoryRepository;
    private readonly IRepository<BusinessInvoice, Guid> _invoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<ProjectPerformanceSnapshot, Guid> _projectPerformanceSnapshotRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public BusinessContextLoader(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<BusinessAcceptance, Guid> acceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> budgetExecutionRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessForecastHistory, Guid> forecastHistoryRepository,
        IRepository<BusinessInvoice, Guid> invoiceRepository,
        IRepository<BusinessProcurement, Guid> procurementRepository,
        IRepository<BusinessReceivable, Guid> receivableRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<ProjectPerformanceSnapshot, Guid> projectPerformanceSnapshotRepository,
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
        _forecastHistoryRepository = forecastHistoryRepository;
        _invoiceRepository = invoiceRepository;
        _procurementRepository = procurementRepository;
        _receivableRepository = receivableRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _projectPerformanceSnapshotRepository = projectPerformanceSnapshotRepository;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<BusinessContext> LoadAsync()
    {
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();

        var allProjects = await _projectRepository.GetListAsync();
        var canSeeAllProjects = _currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo);

        var projects = allProjects
            .Where(project => canSeeAllProjects || accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId))
            .ToList();

        if (_currentUser.Id.HasValue && !canSeeAllProjects)
        {
            var currentUserId = _currentUser.Id.Value;
            var projectIdsByMembership = (await _projectMemberRepository.GetListAsync())
                .Where(item => item.UserId == currentUserId)
                .Select(item => item.ProjectId)
                .ToHashSet();

            projects = projects
                .Concat(allProjects.Where(item =>
                    item.ManagerUserId == currentUserId ||
                    item.SponsorUserId == currentUserId ||
                    projectIdsByMembership.Contains(item.Id)))
                .DistinctBy(item => item.Id)
                .ToList();
        }

        var projectIds = projects.Select(item => item.Id).ToHashSet();
        var users = await _userRepository.GetListAsync();
        var organizationUnits = await _organizationUnitRepository.GetListAsync();

        var acceptances = (await _acceptanceRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var budgetExecutions = (await _budgetExecutionRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var businessChains = (await _businessChainRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var contracts = (await _businessContractRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var forecastHistories = (await _forecastHistoryRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var invoices = (await _invoiceRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var performanceSnapshots = (await _projectPerformanceSnapshotRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var procurements = (await _procurementRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var receivables = (await _receivableRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();

        return new BusinessContext(
            acceptances,
            budgetExecutions,
            businessChains,
            contracts,
            forecastHistories,
            invoices,
            organizationUnits.ToDictionary(item => item.Id, item => item.DisplayName),
            performanceSnapshots,
            procurements,
            projects.ToDictionary(item => item.Id, item => item.ProjectCode),
            projects.ToDictionary(item => item.Id, item => item.Name),
            projects,
            receivables,
            users.ToDictionary(item => item.Id, item => item.Name ?? item.UserName));
    }
}
