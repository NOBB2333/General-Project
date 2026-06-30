using System;
using System.Linq;
using System.Threading.Tasks;
using General.Admin.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace General.Admin.Platform;

[Collection(AdminTestConsts.CollectionDefinitionName)]
public class ProjectWeeklyReportReminderJobHandlerTests : AdminEntityFrameworkCoreTestBase
{
    private readonly ProjectWeeklyReportReminderJobHandler _handler;
    private readonly BusinessReportDistributionJobHandler _businessReportDistributionHandler;
    private readonly BusinessReminderNotificationJobHandler _businessReminderHandler;
    private readonly ProjectReminderNotificationJobHandler _projectReminderHandler;
    private readonly IRepository<AppRequestAuditLog, Guid> _requestAuditLogRepository;

    public ProjectWeeklyReportReminderJobHandlerTests()
    {
        _businessReportDistributionHandler = GetRequiredService<BusinessReportDistributionJobHandler>();
        _businessReminderHandler = GetRequiredService<BusinessReminderNotificationJobHandler>();
        _handler = GetRequiredService<ProjectWeeklyReportReminderJobHandler>();
        _projectReminderHandler = GetRequiredService<ProjectReminderNotificationJobHandler>();
        _requestAuditLogRepository = GetRequiredService<IRepository<AppRequestAuditLog, Guid>>();
    }

    [Fact]
    public async Task ExecuteAsync_Should_Write_Operation_Log()
    {
        string result = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            result = await _handler.ExecuteAsync();
        });

        result.ShouldContain("日志中心");

        await WithUnitOfWorkAsync(async () =>
        {
            var logs = await _requestAuditLogRepository.GetListAsync();
            var log = logs
                .Where(x => x.Url == "/api/app/platform/scheduler/weekly-report-reminder/run")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            log.ShouldNotBeNull();
            log!.MenuTitle.ShouldBe("周报填写通知");
            log.Category.ShouldBe("operation");
            log.HttpMethod.ShouldBe("POST");
            log.HttpStatusCode.ShouldBe(200);
            log.UserName.ShouldBe("system");
            log.HasException.ShouldBeFalse();
            log.ActionSummary.ShouldNotBeNull();
            log.ActionSummary!.ShouldContain("周报提醒完成");
        });
    }

    [Fact]
    public async Task ProjectReminderNotification_Should_Write_Operation_Log()
    {
        string result = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            result = await _projectReminderHandler.ExecuteAsync();
        });

        result.ShouldContain("项目提醒扫描完成");

        await WithUnitOfWorkAsync(async () =>
        {
            var logs = await _requestAuditLogRepository.GetListAsync();
            var log = logs
                .Where(x => x.Url == "/api/app/platform/scheduler/project-reminder-notification/run")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            log.ShouldNotBeNull();
            log!.MenuTitle.ShouldBe("项目到期与风险提醒");
            log.Category.ShouldBe("operation");
            log.HttpMethod.ShouldBe("POST");
            log.HttpStatusCode.ShouldBe(200);
            log.UserName.ShouldBe("system");
            log.HasException.ShouldBeFalse();
            log.ActionSummary.ShouldNotBeNull();
            log.ActionSummary!.ShouldContain("项目提醒扫描完成");
        });
    }

    [Fact]
    public async Task BusinessReminderNotification_Should_Write_Operation_Log()
    {
        string result = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            result = await _businessReminderHandler.ExecuteAsync();
        });

        result.ShouldContain("经营提醒扫描完成");

        await WithUnitOfWorkAsync(async () =>
        {
            var logs = await _requestAuditLogRepository.GetListAsync();
            var log = logs
                .Where(x => x.Url == "/api/app/platform/scheduler/business-reminder-notification/run")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            log.ShouldNotBeNull();
            log!.MenuTitle.ShouldBe("经营提醒与预警推送");
            log.Category.ShouldBe("operation");
            log.HttpMethod.ShouldBe("POST");
            log.HttpStatusCode.ShouldBe(200);
            log.UserName.ShouldBe("system");
            log.HasException.ShouldBeFalse();
            log.ActionSummary.ShouldNotBeNull();
            log.ActionSummary!.ShouldContain("经营提醒扫描完成");
        });
    }

    [Fact]
    public async Task BusinessReportDistribution_Should_Write_Operation_Log()
    {
        string result = null!;

        await WithUnitOfWorkAsync(async () =>
        {
            result = await _businessReportDistributionHandler.ExecuteAsync();
        });

        result.ShouldContain("经营报表分发完成");

        await WithUnitOfWorkAsync(async () =>
        {
            var logs = await _requestAuditLogRepository.GetListAsync();
            var log = logs
                .Where(x => x.Url == "/api/app/platform/scheduler/business-report-distribution/run")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault();

            log.ShouldNotBeNull();
            log!.MenuTitle.ShouldBe("经营报表分发");
            log.Category.ShouldBe("operation");
            log.HttpMethod.ShouldBe("POST");
            log.HttpStatusCode.ShouldBe(200);
            log.UserName.ShouldBe("system");
            log.HasException.ShouldBeFalse();
            log.ActionSummary.ShouldNotBeNull();
            log.ActionSummary!.ShouldContain("经营报表分发完成");
        });
    }
}
