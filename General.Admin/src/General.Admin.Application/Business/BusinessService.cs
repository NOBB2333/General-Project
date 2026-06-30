using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace General.Admin.Business;

public class BusinessService : ITransientDependency
{
    private readonly BusinessContextLoader _contextLoader;
    private readonly BusinessQueryBuilder _queryBuilder;
    private readonly BusinessReportQueryBuilder _reportQueryBuilder;
    private readonly BusinessReportExportBuilder _reportExportBuilder;
    private readonly ICurrentUser _currentUser;

    public BusinessService(
        BusinessContextLoader contextLoader,
        BusinessQueryBuilder queryBuilder,
        BusinessReportQueryBuilder reportQueryBuilder,
        BusinessReportExportBuilder reportExportBuilder,
        ICurrentUser currentUser)
    {
        _contextLoader = contextLoader;
        _queryBuilder = queryBuilder;
        _reportQueryBuilder = reportQueryBuilder;
        _reportExportBuilder = reportExportBuilder;
        _currentUser = currentUser;
    }

    public async Task<BusinessOverviewDto> GetOverviewAsync()
    {
        var context = await _contextLoader.LoadAsync();
        return _queryBuilder.BuildOverview(context, ResolveAccess());
    }

    public async Task<List<BusinessProjectSummaryDto>> GetProjectListAsync(BusinessProjectListInput input)
    {
        var context = await _contextLoader.LoadAsync();
        return _queryBuilder.BuildProjectList(context, input, ResolveAccess());
    }

    public async Task<BusinessProjectDetailDto> GetProjectDetailAsync(Guid projectId)
    {
        var context = await _contextLoader.LoadAsync();
        var project = context.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project == null)
        {
            throw new AbpAuthorizationException("项目不存在或当前用户无权查看经营信息。");
        }

        return _queryBuilder.BuildProjectDetail(context, project, ResolveAccess());
    }

    public async Task<BusinessReportDto> GetReportAsync(BusinessReportQueryInput input)
    {
        var context = await _contextLoader.LoadAsync();
        return _reportQueryBuilder.BuildReport(context, input, ResolveAccess());
    }

    public async Task<BusinessReportExportResultDto> ExportReportAsync(BusinessReportExportInput input)
    {
        var context = await _contextLoader.LoadAsync();
        var report = _reportQueryBuilder.BuildReport(context, input, ResolveAccess());
        return _reportExportBuilder.Build(report, input);
    }

    private BusinessQueryAccess ResolveAccess()
    {
        var isPrivilegedRole = IsPrivilegedRole();
        return new BusinessQueryAccess(
            CanViewProfitAnalysis: isPrivilegedRole,
            CanViewSensitiveAmounts: isPrivilegedRole);
    }

    private bool IsPrivilegedRole()
    {
        return _currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo)
            || _currentUser.IsInRole(PlatformRoleNames.Viewer)
            || _currentUser.IsInRole(PlatformRoleNames.Pm);
    }
}
