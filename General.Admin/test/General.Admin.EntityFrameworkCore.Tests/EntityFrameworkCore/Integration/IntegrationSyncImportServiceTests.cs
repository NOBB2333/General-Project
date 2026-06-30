using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using General.Admin.Project;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Integration;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class IntegrationSyncImportServiceTests : AdminEntityFrameworkCoreTestBase
{
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<IntegrationGroupBinding, Guid> _groupBindingRepository;
    private readonly IRepository<IntegrationIdMapping, Guid> _idMappingRepository;
    private readonly IntegrationSyncImportService _service;
    private readonly IRepository<ProjectMeetingRecord, Guid> _meetingRecordRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;

    public IntegrationSyncImportServiceTests()
    {
        _connectorRepository = GetRequiredService<IRepository<IntegrationConnector, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _groupBindingRepository = GetRequiredService<IRepository<IntegrationGroupBinding, Guid>>();
        _idMappingRepository = GetRequiredService<IRepository<IntegrationIdMapping, Guid>>();
        _meetingRecordRepository = GetRequiredService<IRepository<ProjectMeetingRecord, Guid>>();
        _projectRepository = GetRequiredService<IRepository<ProjectEntity, Guid>>();
        _service = GetRequiredService<IntegrationSyncImportService>();
    }

    [Fact]
    public async Task SyncGroupsAsync_Should_Import_Group_Bindings()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync("feishu");
            var result = await _service.SyncGroupsAsync(
                connector,
                "{}",
                """
                {
                  "groups": [
                    {
                      "chat_id": "oc_project_alpha",
                      "name": "项目 Alpha 群"
                    }
                  ]
                }
                """,
                "飞书");

            result.TotalCount.ShouldBe(1);
            result.SuccessCount.ShouldBe(1);

            var binding = (await _groupBindingRepository.GetListAsync(item =>
                item.ConnectorId == connector.Id &&
                item.ExternalGroupId == "oc_project_alpha")).Single();
            binding.ExternalGroupName.ShouldBe("项目 Alpha 群");
            binding.Status.ShouldBe(IntegrationBindingStatus.Pending);
            binding.InternalObjectId.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task SyncMeetingsAsync_Should_Create_Meeting_Record_And_Id_Mapping()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            var connector = await CreateConnectorAsync("feishu");
            var managerUserId = Guid.NewGuid();
            var project = new ProjectEntity(
                Guid.NewGuid(),
                $"PRJ-{Guid.NewGuid():N}"[..16],
                "外部会议同步项目",
                "会议同步",
                "交付项目",
                "内部立项",
                Guid.NewGuid(),
                managerUserId,
                Guid.NewGuid(),
                ProjectPriorities.P1,
                ProjectStatuses.InProgress,
                DateTime.Today.AddDays(-3),
                DateTime.Today.AddMonths(1),
                isKeyProject: false,
                description: "用于验证外部会议同步。");
            await _projectRepository.InsertAsync(project, autoSave: true);

            var result = await _service.SyncMeetingsAsync(
                connector,
                "{}",
                $$"""
                  {
                    "meetings": [
                      {
                        "meeting_id": "meeting-alpha",
                        "projectId": "{{project.Id}}",
                        "topic": "周例会",
                        "meetingTime": "2026-06-26 10:00:00",
                        "minutes": "完成风险梳理。",
                        "decisions": "下周关闭 P1 风险。"
                      }
                    ]
                  }
                  """,
                "飞书");

            result.TotalCount.ShouldBe(1);
            result.SuccessCount.ShouldBe(1);

            var meeting = (await _meetingRecordRepository.GetListAsync(item => item.ProjectId == project.Id)).Single();
            meeting.Topic.ShouldBe("周例会");
            meeting.FacilitatorUserId.ShouldBe(managerUserId);
            meeting.Minutes.ShouldContain("风险梳理");

            var mapping = (await _idMappingRepository.GetListAsync(item =>
                item.ConnectorId == connector.Id &&
                item.ExternalObjectType == "ExternalMeeting" &&
                item.ExternalObjectId == "meeting-alpha")).Single();
            mapping.InternalObjectType.ShouldBe("ProjectMeetingRecord");
            mapping.InternalObjectId.ShouldBe(meeting.Id.ToString());
        });
    }

    private async Task<IntegrationConnector> CreateConnectorAsync(string connectorCode)
    {
        var connector = new IntegrationConnector(
            Guid.NewGuid(),
            connectorCode,
            "Feishu",
            "飞书",
            "{}",
            """["SyncDepartment","SyncUser","SyncGroup","SyncMeeting"]""",
            _currentTenant.Id,
            isBuiltin: false,
            isEnabled: true,
            icon: "test",
            sort: 999,
            remark: "用于验证外部系统同步导入。");
        await _connectorRepository.InsertAsync(connector, autoSave: true);
        return connector;
    }
}
