using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

[ExposeServices(typeof(IFileBusinessLinkResolver))]
public class ProjectFileBusinessLinkResolver : IFileBusinessLinkResolver
{
    private static readonly HashSet<string> SupportedBusinessTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Project",
        "ProjectSprint",
        "ProjectRequirement",
        "ProjectTask",
        "ProjectReport",
        "ProjectMeetingRecord"
    };

    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;

    public ProjectFileBusinessLinkResolver(
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectMeetingRecord, Guid> projectMeetingRecordRepository,
        IRepository<ProjectReport, Guid> projectReportRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository)
    {
        _projectSprintRepository = projectSprintRepository;
        _projectMeetingRecordRepository = projectMeetingRecordRepository;
        _projectReportRepository = projectReportRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectTaskRepository = projectTaskRepository;
    }

    public bool CanResolve(string businessType)
    {
        return SupportedBusinessTypes.Contains(businessType.Trim());
    }

    public async Task<Guid?> ResolveProjectIdAsync(string businessType, string businessId)
    {
        if (!Guid.TryParse(businessId, out var id))
        {
            return null;
        }

        return businessType.Trim() switch
        {
            var type when type.Equals("Project", StringComparison.OrdinalIgnoreCase) => id,
            var type when type.Equals("ProjectSprint", StringComparison.OrdinalIgnoreCase) => (await _projectSprintRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("ProjectRequirement", StringComparison.OrdinalIgnoreCase) => (await _projectRequirementRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("ProjectTask", StringComparison.OrdinalIgnoreCase) => (await _projectTaskRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("ProjectReport", StringComparison.OrdinalIgnoreCase) => (await _projectReportRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("ProjectMeetingRecord", StringComparison.OrdinalIgnoreCase) => (await _projectMeetingRecordRepository.FindAsync(id))?.ProjectId,
            _ => null
        };
    }
}
