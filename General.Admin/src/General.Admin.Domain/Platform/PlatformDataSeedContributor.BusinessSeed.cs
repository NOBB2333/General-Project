using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor
{
    private async Task SeedBusinessManagementDataAsync()
    {
        var pmoUser = await _userManager.FindByNameAsync("pmo.demo");
        var pmUser = await _userManager.FindByNameAsync("pm.demo");
        var viewerUser = await _userManager.FindByNameAsync("viewer.demo");

        if (pmoUser == null || pmUser == null || viewerUser == null)
        {
            return;
        }

        var projects = (await _projectRepository.GetListAsync())
            .OrderBy(x => x.ProjectCode)
            .ToList();

        if (projects.Count < 3)
        {
            return;
        }

        if (!await _businessBudgetExecutionRepository.AnyAsync())
        {
            await _businessBudgetExecutionRepository.InsertManyAsync(
            [
                new(Guid.Parse("61000000-0000-0000-0000-000000000001"), projects[0].Id, "BGT-PRJ-2026-001-01", "软件开发", 620000m, 580000m, -40000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000002"), projects[0].Id, "BGT-PRJ-2026-001-02", "现场实施", 340000m, 410000m, 70000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000003"), projects[0].Id, "BGT-PRJ-2026-001-03", "分包采购", 240000m, 360000m, 120000m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000004"), projects[0].Id, "BGT-PRJ-2026-001-04", "项目管理", 300000m, 290000m, -10000m, 40),
                new(Guid.Parse("61000000-0000-0000-0000-000000000005"), projects[1].Id, "BGT-PRJ-2026-002-01", "平台开发", 280000m, 320000m, 40000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000006"), projects[1].Id, "BGT-PRJ-2026-002-02", "接口联调", 220000m, 210000m, -10000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000007"), projects[1].Id, "BGT-PRJ-2026-002-03", "分包采购", 180000m, 180000m, 0m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000008"), projects[1].Id, "BGT-PRJ-2026-002-04", "测试发布", 220000m, 300000m, 80000m, 40),
                new(Guid.Parse("61000000-0000-0000-0000-000000000009"), projects[2].Id, "BGT-PRJ-2026-003-01", "接口联调", 160000m, 110000m, -50000m, 10),
                new(Guid.Parse("61000000-0000-0000-0000-000000000010"), projects[2].Id, "BGT-PRJ-2026-003-02", "商务支持", 140000m, 120000m, -20000m, 20),
                new(Guid.Parse("61000000-0000-0000-0000-000000000011"), projects[2].Id, "BGT-PRJ-2026-003-03", "分包采购", 90000m, 90000m, 0m, 30),
                new(Guid.Parse("61000000-0000-0000-0000-000000000012"), projects[2].Id, "BGT-PRJ-2026-003-04", "交付支持", 210000m, 200000m, -10000m, 40)
            ], autoSave: true);
        }

        if (!await _businessContractRepository.AnyAsync())
        {
            await _businessContractRepository.InsertManyAsync(
            [
                new(Guid.Parse("62000000-0000-0000-0000-000000000001"), projects[0].Id, "REV-PRJ-2026-001-001", "智慧园区一期主合同", "主合同", "华东智慧园区集团", 1800000m, new DateTime(2026, 3, 30), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000002"), projects[0].Id, "REV-PRJ-2026-001-002", "智慧园区一期补充合同 A", "补充合同", "华东智慧园区集团", 260000m, new DateTime(2026, 5, 18), "拟签订", true, "REV-PRJ-2026-001-001", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000003"), projects[1].Id, "REV-PRJ-2026-002-001", "运营中台主合同", "主合同", "远航运营科技", 1180000m, new DateTime(2026, 2, 15), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000004"), projects[1].Id, "REV-PRJ-2026-002-002", "运营中台补充合同 B", "补充合同", "远航运营科技", 140000m, new DateTime(2026, 4, 20), "已签订", true, "REV-PRJ-2026-002-001", "CHG-PRJ-2026-002-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000005"), projects[2].Id, "REV-PRJ-2026-003-001", "财务共享平台主合同", "主合同", "财务共享中心", 820000m, new DateTime(2026, 4, 1), "执行中", true),
                new(Guid.Parse("62000000-0000-0000-0000-000000000006"), projects[0].Id, "PROC-PRJ-2026-001-001", "弱电集成分包合同", "分包合同", "申城弱电科技", 240000m, new DateTime(2026, 4, 12), "执行中", false, "REV-PRJ-2026-001-001"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000007"), projects[0].Id, "PROC-PRJ-2026-001-002", "视频巡检外采协议", "采购合同", "沪上视频巡检服务商", 120000m, new DateTime(2026, 5, 21), "待签订", false, "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000008"), projects[1].Id, "PROC-PRJ-2026-002-001", "报表开发分包合同", "分包合同", "浦江数据服务", 180000m, new DateTime(2026, 3, 19), "已签订", false, "REV-PRJ-2026-002-001"),
                new(Guid.Parse("62000000-0000-0000-0000-000000000009"), projects[2].Id, "PROC-PRJ-2026-003-001", "接口治理服务预留", "采购合同", "共享接口服务商", 90000m, new DateTime(2026, 4, 22), "待确认", false, "REV-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessChainRepository.AnyAsync())
        {
            await _businessChainRepository.InsertManyAsync(
            [
                new(Guid.Parse("63000000-0000-0000-0000-000000000001"), projects[0].Id, "BIZ-PRJ-2026-001-001", "投标链", "对外投标", "园区一期招标应答", "已中标", pmUser.Id, "已完成应答材料、现场答疑和中标通知归档。", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000002"), projects[0].Id, "BIZ-PRJ-2026-001-002", "分包链", "外采分包", "弱电集成分包", "执行中", pmUser.Id, "弱电集成能力由第三方分包执行，金额按主合同 4.2 条拆分。", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000003"), projects[0].Id, "BIZ-PRJ-2026-001-003", "变更链", "合同变更", "新增视频巡检范围", "评审中", pmoUser.Id, "客户新增视频巡检范围，准备形成补充合同并同步调整分包范围。", "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000004"), projects[1].Id, "BIZ-PRJ-2026-002-001", "投标链", "对外投标", "运营中台采购应答", "已中标", pmUser.Id, "通过客户统一采购流程中标，已补齐投标应答与澄清纪要。", "REV-PRJ-2026-002-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000005"), projects[1].Id, "BIZ-PRJ-2026-002-002", "分包链", "外采分包", "报表开发分包", "已签订", pmUser.Id, "报表开发工作拆分给合作方执行，并保留二次验收条款。", "PROC-PRJ-2026-002-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000006"), projects[2].Id, "BIZ-PRJ-2026-003-001", "投标链", "对外投标", "财务共享平台采购应答", "已中标", viewerUser.Id, "客户统一采购流程已结束，中标通知书已挂接至项目。", "REV-PRJ-2026-003-001"),
                new(Guid.Parse("63000000-0000-0000-0000-000000000007"), projects[2].Id, "BIZ-PRJ-2026-003-002", "分包链", "外采预留", "接口治理外采预留", "待确认", viewerUser.Id, "预留与外部厂商的接口治理外采，不在当前 Sprint 签订。", "PROC-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessProcurementRepository.AnyAsync())
        {
            await _businessProcurementRepository.InsertManyAsync(
            [
                new(Guid.Parse("64000000-0000-0000-0000-000000000001"), projects[0].Id, "PRC-PRJ-2026-001-001", "弱电集成分包合同", "申城弱电科技", 240000m, new DateTime(2026, 4, 12), "分包执行", "执行中", "REV-PRJ-2026-001-001"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000002"), projects[0].Id, "PRC-PRJ-2026-001-002", "视频巡检外采协议", "沪上视频巡检服务商", 120000m, new DateTime(2026, 5, 21), "拟采购", "待签订", "REV-PRJ-2026-001-002", "CHG-PRJ-2026-001-01"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000003"), projects[1].Id, "PRC-PRJ-2026-002-001", "报表开发分包合同", "浦江数据服务", 180000m, new DateTime(2026, 3, 19), "分包执行", "已签订", "REV-PRJ-2026-002-001"),
                new(Guid.Parse("64000000-0000-0000-0000-000000000004"), projects[2].Id, "PRC-PRJ-2026-003-001", "接口治理服务预留", "共享接口服务商", 90000m, new DateTime(2026, 4, 22), "预留分包", "待确认", "REV-PRJ-2026-003-001")
            ], autoSave: true);
        }

        if (!await _businessReceivableRepository.AnyAsync())
        {
            await _businessReceivableRepository.InsertManyAsync(
            [
                new(Guid.Parse("65000000-0000-0000-0000-000000000001"), projects[0].Id, "REC-PRJ-2026-001-001", "首期回款", new DateTime(2026, 5, 15), 900000m, 720000m, "部分到账", "REV-PRJ-2026-001-001", "INV-PRJ-2026-001-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000002"), projects[0].Id, "REC-PRJ-2026-001-002", "二期验收回款", new DateTime(2026, 8, 30), 620000m, 0m, "待回款", "REV-PRJ-2026-001-002", "INV-PRJ-2026-001-002"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000003"), projects[1].Id, "REC-PRJ-2026-002-001", "一期上线回款", new DateTime(2026, 4, 28), 840000m, 760000m, "部分到账", "REV-PRJ-2026-002-001", "INV-PRJ-2026-002-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000004"), projects[1].Id, "REC-PRJ-2026-002-002", "补充合同回款", new DateTime(2026, 9, 15), 340000m, 0m, "待回款", "REV-PRJ-2026-002-002", "INV-PRJ-2026-002-002"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000005"), projects[2].Id, "REC-PRJ-2026-003-001", "启动款", new DateTime(2026, 5, 30), 320000m, 280000m, "部分到账", "REV-PRJ-2026-003-001", "INV-PRJ-2026-003-001"),
                new(Guid.Parse("65000000-0000-0000-0000-000000000006"), projects[2].Id, "REC-PRJ-2026-003-002", "收尾尾款", new DateTime(2026, 12, 15), 400000m, 0m, "待回款", "REV-PRJ-2026-003-001", "INV-PRJ-2026-003-002")
            ], autoSave: true);
        }

        if (!await _businessForecastHistoryRepository.AnyAsync())
        {
            await _businessForecastHistoryRepository.InsertManyAsync(
            [
                new(Guid.Parse("66000000-0000-0000-0000-000000000001"), projects[0].Id, "年底预计回款", "138.00 万", "152.00 万", viewerUser.Id, "客户确认首期验收后回款时间前置", "回款预测", "REC-PRJ-2026-001-001"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000002"), projects[0].Id, "毛利率预测", "32.4%", "28.9%", pmoUser.Id, "新增弱电分包导致外采成本上升", "利润预测", "PRC-PRJ-2026-001-001"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000003"), projects[1].Id, "年底预计回款", "105.00 万", "118.00 万", pmUser.Id, "客户确认补充合同将于 Q3 一并回款", "回款预测", "REC-PRJ-2026-002-002"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000004"), projects[1].Id, "利润预测", "36.00 万", "31.00 万", pmoUser.Id, "测试发布成本超出预算", "利润预测", "BGT-PRJ-2026-002-04"),
                new(Guid.Parse("66000000-0000-0000-0000-000000000005"), projects[2].Id, "年底预计回款", "60.00 万", "72.00 万", viewerUser.Id, "客户确认联调结束后可释放第二笔回款", "回款预测", "REC-PRJ-2026-003-002")
            ], autoSave: true);
        }
    }
}
