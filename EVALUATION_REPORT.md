# General Admin 项目未修复问题清单

> 评估日期：2026-06-15
> 来源：基于 `EVALUATION_REPORT.md` 核对当前代码后的剩余未修复项

---

## 后端未修复问题

| 优先级 | 问题 | 文件 | 建议 |
|--------|------|------|------|
| P1 | **BusinessMutationService重复权限逻辑** | `BusinessMutationService.cs:217-249` | 复用`ProjectAccessGuard` |
| P1 | **ProjectContextLoader注入23个仓库** | `ProjectContextLoader.cs` | 拆分为子加载器 |
| P1 | **Mapperly已引入但未使用** | `AdminApplicationMappers.cs` | 实现自动映射 |
| P2 | **无Update操作** | 大部分实体 | 补齐CRUD完整生命周期 |
| P2 | **HttpApi层未使用** | `General.Admin.HttpApi` | 遵循ABP标准模式 |

## 前端未修复问题

| 优先级 | 问题 | 文件 | 建议 |
|--------|------|------|------|
| P1 | **composable过大** | `useProjectDetailPage.ts` (1629行) | 拆分为更小的子composable |
| P1 | **共享组件偏少** | `src/components/` | 提取MetricsDashboard、StatusTag等复用组件 |
| P1 | **甘特图/日历为原生实现** | `ProjectTasksTab.vue` | 功能基础，不支持拖拽、缩放、依赖线 |
| P2 | **无单元测试** | `test/`目录 | 建立测试框架和用例 |
| P2 | **无loading/skeleton统一方案** | 各页面 | 统一加载状态展示 |
| P2 | **无搜索/过滤URL同步** | 各列表页 | 筛选条件同步到URL |
| P2 | **无批量操作** | 多个表格 | 补齐批量选择和操作 |
| P2 | **无导出功能** | 各列表页 | 补齐CSV/Excel导出 |

---

> 已修复项（P0 安全/性能问题、ProjectService 拆分、状态常量类等）已从本清单移除。
