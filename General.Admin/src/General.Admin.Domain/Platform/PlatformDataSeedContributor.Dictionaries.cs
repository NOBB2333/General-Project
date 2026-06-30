using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor
{
    private async Task SeedFileStorageSourcesAsync()
    {
        if (await _fileStorageSourceRepository.AnyAsync())
        {
            return;
        }

        await _fileStorageSourceRepository.InsertAsync(
            new AppFileStorageSource(
                Guid.Parse("69000000-0000-0000-0000-000000000001"),
                "本地默认",
                PlatformFileStorageNames.Local,
                null,
                "App_Data/upload-files",
                string.Empty,
                null,
                null,
                null,
                null,
                string.Empty,
                false,
                true,
                true,
                false,
                "平台默认本地文件存储源。"),
            autoSave: true);
    }

    private async Task SeedDictionariesAsync()
    {
        var projectStatus = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000001", "project_status", "项目状态", 1, "项目执行状态下拉。");
        var taskPriority = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000002", "task_priority", "任务优先级", 2, "任务优先级标签。");
        var fileCategory = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000003", "file_category", "文件分类", 3, "平台文件分类。");
        var taskStatus = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000004", "task_status", "任务状态", 4, "项目任务执行状态。");
        var raidType = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000005", "raid_type", "RAID 事项类型", 5, "项目 RAID 事项分类。");
        var projectType = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000006", "project_type", "项目类型", 6, "项目立项和筛选使用的项目类型。");
        var projectSource = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000007", "project_source", "项目来源", 7, "项目发起来源。");
        var contractType = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000008", "contract_type", "合同类型", 8, "经营合同分类。");
        var changeStatus = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000009", "change_status", "变更状态", 9, "项目变更请求状态。");
        var businessCloseStatus = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000010", "business_close_status", "经营关闭状态", 10, "经营执行和收尾状态。");
        var approvalStatus = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000011", "approval_status", "审批状态", 11, "章程和审批类业务状态。");
        var priority = await EnsureDictTypeAsync("67000000-0000-0000-0000-000000000012", "priority", "优先级", 12, "项目、任务、风险等通用优先级。");

        await EnsureDictDataAsync(projectStatus, "68000000-0000-0000-0000-000000000001", "待规划", "待规划", 1, "default");
        await EnsureDictDataAsync(projectStatus, "68000000-0000-0000-0000-000000000002", "进行中", "进行中", 2, "processing");
        await EnsureDictDataAsync(projectStatus, "68000000-0000-0000-0000-000000000003", "已完成", "已完成", 3, "success");
        await EnsureDictDataAsync(projectStatus, "68000000-0000-0000-0000-000000000101", "已关闭", "已关闭", 4, "default");

        await EnsureDictDataAsync(taskPriority, "68000000-0000-0000-0000-000000000006", "P0", "P0", 1, "error");
        await EnsureDictDataAsync(taskPriority, "68000000-0000-0000-0000-000000000005", "P1", "P1", 2, "warning");
        await EnsureDictDataAsync(taskPriority, "68000000-0000-0000-0000-000000000004", "P2", "P2", 3, "blue");
        await EnsureDictDataAsync(taskPriority, "68000000-0000-0000-0000-00000000000D", "P3", "P3", 4, "default");

        await EnsureDictDataAsync(fileCategory, "68000000-0000-0000-0000-000000000007", "默认", "default", 1);
        await EnsureDictDataAsync(fileCategory, "68000000-0000-0000-0000-000000000008", "合同", "contract", 2);
        await EnsureDictDataAsync(fileCategory, "68000000-0000-0000-0000-000000000009", "交付", "delivery", 3);

        await EnsureDictDataAsync(taskStatus, "68000000-0000-0000-0000-000000000111", "未开始", "未开始", 1, "default");
        await EnsureDictDataAsync(taskStatus, "68000000-0000-0000-0000-000000000112", "进行中", "进行中", 2, "processing");
        await EnsureDictDataAsync(taskStatus, "68000000-0000-0000-0000-000000000113", "阻塞", "阻塞", 3, "error");
        await EnsureDictDataAsync(taskStatus, "68000000-0000-0000-0000-000000000114", "已完成", "已完成", 4, "success");
        await EnsureDictDataAsync(taskStatus, "68000000-0000-0000-0000-000000000115", "已关闭", "已关闭", 5, "default");

        await EnsureDictDataAsync(raidType, "68000000-0000-0000-0000-000000000116", "风险", "风险", 1, "error");
        await EnsureDictDataAsync(raidType, "68000000-0000-0000-0000-000000000117", "问题", "问题", 2, "warning");
        await EnsureDictDataAsync(raidType, "68000000-0000-0000-0000-000000000118", "依赖", "依赖", 3, "blue");
        await EnsureDictDataAsync(raidType, "68000000-0000-0000-0000-000000000119", "假设", "假设", 4, "default");

        await EnsureDictDataAsync(projectType, "68000000-0000-0000-0000-000000000120", "平台建设", "平台建设", 1);
        await EnsureDictDataAsync(projectType, "68000000-0000-0000-0000-000000000121", "业务项目", "业务项目", 2);
        await EnsureDictDataAsync(projectType, "68000000-0000-0000-0000-000000000122", "客户交付", "客户交付", 3);
        await EnsureDictDataAsync(projectType, "68000000-0000-0000-0000-000000000123", "内部优化", "内部优化", 4);

        await EnsureDictDataAsync(projectSource, "68000000-0000-0000-0000-000000000124", "公司立项", "公司立项", 1);
        await EnsureDictDataAsync(projectSource, "68000000-0000-0000-0000-000000000125", "客户需求", "客户需求", 2);
        await EnsureDictDataAsync(projectSource, "68000000-0000-0000-0000-000000000126", "部门提报", "部门提报", 3);
        await EnsureDictDataAsync(projectSource, "68000000-0000-0000-0000-000000000127", "经营规划", "经营规划", 4);

        await EnsureDictDataAsync(contractType, "68000000-0000-0000-0000-000000000128", "主合同", "主合同", 1);
        await EnsureDictDataAsync(contractType, "68000000-0000-0000-0000-000000000129", "补充合同", "补充合同", 2);
        await EnsureDictDataAsync(contractType, "68000000-0000-0000-0000-000000000130", "分包合同", "分包合同", 3);
        await EnsureDictDataAsync(contractType, "68000000-0000-0000-0000-000000000131", "采购合同", "采购合同", 4);

        await EnsureDictDataAsync(changeStatus, "68000000-0000-0000-0000-000000000132", "待评审", "待评审", 1, "default");
        await EnsureDictDataAsync(changeStatus, "68000000-0000-0000-0000-000000000133", "审批中", "审批中", 2, "processing");
        await EnsureDictDataAsync(changeStatus, "68000000-0000-0000-0000-000000000134", "已批准", "已批准", 3, "success");
        await EnsureDictDataAsync(changeStatus, "68000000-0000-0000-0000-000000000135", "已驳回", "已驳回", 4, "error");
        await EnsureDictDataAsync(changeStatus, "68000000-0000-0000-0000-000000000136", "已关闭", "已关闭", 5, "default");

        await EnsureDictDataAsync(businessCloseStatus, "68000000-0000-0000-0000-000000000137", "经营执行中", "经营执行中", 1, "processing");
        await EnsureDictDataAsync(businessCloseStatus, "68000000-0000-0000-0000-000000000138", "经营收尾中", "经营收尾中", 2, "warning");
        await EnsureDictDataAsync(businessCloseStatus, "68000000-0000-0000-0000-000000000139", "执行关闭待回款", "执行关闭待回款", 3, "gold");

        await EnsureDictDataAsync(approvalStatus, "68000000-0000-0000-0000-000000000140", "草稿", "草稿", 1, "default");
        await EnsureDictDataAsync(approvalStatus, "68000000-0000-0000-0000-000000000141", "待审批", "待审批", 2, "processing");
        await EnsureDictDataAsync(approvalStatus, "68000000-0000-0000-0000-000000000142", "已批准", "已批准", 3, "success");
        await EnsureDictDataAsync(approvalStatus, "68000000-0000-0000-0000-000000000143", "退回", "退回", 4, "error");

        await EnsureDictDataAsync(priority, "68000000-0000-0000-0000-000000000144", "P0", "P0", 1, "error");
        await EnsureDictDataAsync(priority, "68000000-0000-0000-0000-000000000145", "P1", "P1", 2, "warning");
        await EnsureDictDataAsync(priority, "68000000-0000-0000-0000-000000000146", "P2", "P2", 3, "blue");
        await EnsureDictDataAsync(priority, "68000000-0000-0000-0000-000000000147", "P3", "P3", 4, "default");
    }

    private async Task<AppDictType> EnsureDictTypeAsync(string id, string code, string name, int sort, string remark)
    {
        var normalizedCode = AppDictType.NormalizeCode(code);
        var existing = await _dictTypeRepository.FindAsync(item => item.Code == normalizedCode);
        if (existing != null)
        {
            return existing;
        }

        var entity = new AppDictType(Guid.Parse(id), normalizedCode, name, sort, remark);
        await _dictTypeRepository.InsertAsync(entity, autoSave: true);
        return entity;
    }

    private async Task EnsureDictDataAsync(
        AppDictType dictType,
        string id,
        string label,
        string value,
        int sort,
        string? tagColor = null,
        string? remark = null)
    {
        var dataId = Guid.Parse(id);
        var existingByValue = await _dictDataRepository.FindAsync(item => item.DictTypeId == dictType.Id && item.Value == value);
        var existingById = await _dictDataRepository.FindAsync(dataId);
        if (existingByValue != null)
        {
            existingByValue.Update(label, value, sort, existingByValue.IsEnabled, tagColor, remark);
            await _dictDataRepository.UpdateAsync(existingByValue, autoSave: true);

            if (existingById != null && existingById.Id != existingByValue.Id && existingById.DictTypeId == dictType.Id)
            {
                existingById.Update(existingById.Label, existingById.Value, existingById.Sort, false, existingById.TagColor, existingById.Remark);
                await _dictDataRepository.UpdateAsync(existingById, autoSave: true);
            }

            return;
        }

        if (existingById != null)
        {
            if (existingById.DictTypeId == dictType.Id)
            {
                existingById.Update(label, value, sort, true, tagColor, remark);
                await _dictDataRepository.UpdateAsync(existingById, autoSave: true);
                return;
            }

            dataId = _guidGenerator.Create();
        }

        await _dictDataRepository.InsertAsync(
            new AppDictData(dataId, dictType.Id, label, value, sort, tagColor: tagColor, remark: remark),
            autoSave: true);
    }
}
