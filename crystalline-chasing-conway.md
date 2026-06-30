# Card + VXE Grid 间距统一规范与修复清单

## 一、我们的标准是什么

### 1.1 核心原则

> **所有页面统一使用 Tailwind CSS 工具类控制间距、边框、圆角、阴影。**
> **不再使用 `scoped style` 硬编码 `margin / padding / gap / border-radius`，不再使用旧的全局类 `app-vxe-card-grid--flush / --section / --embedded`。**

### 1.2 Card 包裹 VXE Grid 的标准模式

```vue
<template>
  <Page title="页面标题" description="页面描述">
    <section class="flex flex-col gap-4">
      <!-- 1. 页面主体 Card：统一无框、圆角一致 -->
      <Card :bordered="false" class="rounded-2xl">
        <!-- 2. 如果 Card 内只有表格，表格本身不需要额外 class -->
        <!-- 3. 如果 Card 内除了表格还有其他元素，用 div 包裹表格并控制间距 -->
        <div v-if="showToolbar" class="mb-4">...</div>
        <Grid :grid-options="gridOptions" :grid-events="gridEvents" />
      </Card>
    </section>
  </Page>
</template>

<style scoped>
/* 4. 不再写任何 margin / padding / gap / border-radius 覆盖 */
</style>
```

### 1.3 具体要求

| 维度 | 标准要求 | 反例（不标准） |
|------|---------|--------------|
| **间距** | 使用 Tailwind：`p-4`、`pb-4`、`mb-4`、`gap-4`、`space-y-4` | `margin-bottom: 12px`、`padding-top: 12px`、`gap: 16px` |
| **边框** | Card 统一 `:bordered="false"` | 默认带边框或部分有、部分无 |
| **圆角** | 使用 Tailwind：`rounded-2xl` / `rounded-xl`（统一设计 token） | `border-radius: 12px`、`border-radius: 20px` |
| **表格位置** | 表格在 Card body padding 内，不突破 padding | 使用负边距 `margin: -5px -24px -24px` 抵消 padding |
| **分页器间距** | 分页器与表格最后一行保持 `12px` 间距，与 Card 底部保持 `24px` 间距 | 分页器贴到 Card 底部 |
| **无分页表格** | 表格最后一行与 Card 底部保持 `24px` 间距 | 表格底部贴到 Card 下边缘 |
| **工具类来源** | 只用 Tailwind + ant-design-vue 组件默认样式 | 自定义全局 CSS 类如 `app-vxe-card-grid--flush` |

### 1.4 间距标准值

| 场景 | Tailwind 类 | 对应像素 |
|------|------------|---------|
| 页面级 section 间距 | `gap-4` | 16px |
| 卡片内元素间距 | `gap-3`、`space-y-3` | 12px |
| 表格上方工具栏间距 | `mb-4` | 16px |
| 表格下方与分页器间距 | 由 VXE Grid 默认 `padding-top: 8px` + 设计保留 | 约 8-12px |
| 卡片 body 默认 padding | 不覆盖，使用 antd 默认 | 24px |
| 小间距 | `gap-2` | 8px |

---

## 二、为什么不标准 / 不合理

### 2.1 `app-vxe-card-grid--flush` 的问题

旧类定义：

```css
.app-vxe-card-grid--flush {
  margin: -5px -24px -24px;
}
```

`-24px` 底部负边距会**抵消 ant-design-vue Card body 的默认 24px padding**。后果：

- **有分页的表格**：分页器被推到卡片下边缘，视觉上"贴底"。
- **无分页的表格**：表格最后一行直接贴到卡片下边缘。
- **不同页面表现不一致**：有的页面用 `--flush`，有的不用，有的用 `--section`，用户感知到的间距完全不同。

### 2.2 `app-vxe-card-grid--section` / `--embedded` 的问题

```css
.app-vxe-card-grid--section { margin: 12px -24px -24px; }
.app-vxe-card-grid--embedded { margin-top: 8px; }
```

- `--section` 同样会抵消底部 24px padding，只是顶部多了 12px 正边距。
- `--embedded` 只有顶部 8px，无法控制底部。
- 三个类之间没有文档说明，开发者凭感觉选择，导致全系统混乱。

### 2.3 `scoped style` 硬编码的问题

示例：

```css
.platform-audit__query-card :deep(.ant-card-body) {
  padding-right: 0;
  padding-bottom: 0;
  padding-left: 0;
}
.platform-audit__query-card :deep(.vxe-grid) {
  margin-top: -5px;
}
```

问题：
- 破坏组件封装，直接覆盖 ant-design-vue 内部样式。
- 不同页面覆盖方式不同，无法保证一致性。
- 后续升级 ant-design-vue 或 VXE Table 时容易失效。
- 无法被设计系统统一管控。

### 2.4 Card `bordered` 不统一的问题

- 项目核心页面（users、projects）统一使用 `:bordered="false"`。
- Integration 模块所有页面都使用默认带边框 Card。
- AI 模块有的有、有的无。
- 同一系统内卡片外观不一致，显得粗糙。

### 2.5 圆角不统一的问题

项目中同时存在：
- `border-radius: 20px`
- `border-radius: 12px`
- `border-radius: 6px`
- Tailwind 未使用

同一系统内卡片圆角不一致，破坏视觉统一性。

---

## 三、标准的应该是什么样子

### 3.1 标准列表页示例

```vue
<script lang="ts" setup>
import { Page } from '@vben/common-ui';
import { Card } from 'ant-design-vue';
import { useVbenVxeGrid } from '#/adapter/vxe-table';

const [Grid] = useVbenVxeGrid({ gridOptions: {} });
</script>

<template>
  <Page title="用户管理" description="管理用户账号与权限">
    <section class="flex flex-col gap-4">
      <Card :bordered="false" class="rounded-2xl">
        <Grid :grid-options="gridOptions" :grid-events="gridEvents" />
      </Card>
    </section>
  </Page>
</template>

<!-- 不再写 scoped style -->
```

### 3.2 标准列表页（带工具栏）示例

```vue
<template>
  <Page title="文件管理" description="管理平台文件">
    <section class="flex flex-col gap-4">
      <Card :bordered="false" class="rounded-2xl">
        <div class="mb-4 flex items-center justify-end gap-3">
          <Input placeholder="搜索文件" />
          <Button type="primary">上传</Button>
        </div>
        <Grid :grid-options="gridOptions" :grid-events="gridEvents" />
      </Card>
    </section>
  </Page>
</template>
```

### 3.3 标准详情 Tab 页示例

```vue
<template>
  <section class="flex flex-col gap-4">
    <Card :bordered="false" class="rounded-2xl" title="缺陷 / 问题">
      <div class="mb-4 flex items-center justify-between">
        <span class="text-sm text-secondary">缺陷用于记录 Bug 反馈、定位、修复和验证</span>
        <Button type="primary">新建问题</Button>
      </div>
      <Grid :grid-options="gridOptions" />
    </Card>
  </section>
</template>
```

### 3.4 标准 Card 内 metrics 示例

```vue
<template>
  <section class="grid grid-cols-4 gap-4">
    <Card v-for="metric in metrics" :key="metric.label" :bordered="false" class="rounded-2xl">
      <div class="flex flex-col gap-2 p-2">
        <span class="text-sm text-secondary">{{ metric.label }}</span>
        <strong class="text-3xl">{{ metric.value }}</strong>
      </div>
    </Card>
  </section>
</template>
```

---

## 四、全量修复清单

### 4.1 全局样式文件（1 个）

| 文件路径 | 问题 | 修复方式 |
|---------|------|---------|
| `General.Web/apps/web-general/src/styles/app-vxe.css` | 定义了 `app-vxe-card-grid--flush/section/embedded` 负边距类 | 删除这些负边距类，或仅保留无负边距的 `app-vxe-card-grid` 基础类作为兼容 |

### 4.2 平台管理模块（Platform）

| 文件路径 | 旧类 / 硬编码 | 具体问题 | 修复要求 |
|---------|-------------|---------|---------|
| `platform/users/index.vue` | `app-vxe-card-grid--flush` | 用户列表分页器贴底 | 移除 `--flush`，Card 保持 `:bordered="false" class="rounded-2xl"` |
| `platform/files/index.vue` | `app-vxe-card-grid--flush` | 文件清单分页器贴底 | 移除 `--flush`；`border-radius: 12px`、`min-height: 640px` 改为 Tailwind |
| `platform/scheduler/index.vue` | `app-vxe-card-grid--flush` | 任务中心分页器贴底 | 移除 `--flush`；`.scheduler__metrics { margin-bottom: 16px }` 改为 `mb-4` |
| `platform/roles/index.vue` | `app-vxe-card-grid--flush` | 角色列表分页器贴底 | 移除 `--flush`；Card 添加 `:bordered="false"`；`border-radius: 20px` 改为 `rounded-2xl` |
| `platform/configs/index.vue` | `app-vxe-card-grid--flush` | 参数列表分页器贴底 | 移除 `--flush`；Card 添加 `:bordered="false" class="rounded-2xl"` |
| `platform/cache/index.vue` | `app-vxe-card-grid--flush` | 缓存域分页器贴底 | 移除 `--flush`；Card 添加 `:bordered="false" class="rounded-2xl"` |
| `platform/audit-logs/index.vue` | 硬编码 `.platform-audit__query-card :deep(.ant-card-body) { padding: 0 }`、`.platform-audit__query-card :deep(.vxe-grid) { margin-top: -5px }` | 日志查询 Card padding 被清零、Grid 负 margin | 删除这些 `:deep()` 覆盖；如需要调整间距，用 Tailwind `p-4` / `mb-4` |
| `platform/organization/index.vue` | 未显式用旧类，但 Grid 作为 Card body 唯一子元素可能被全局规则 `.ant-card-body>.vben-vxe-grid:first-child:last-child` 自动应用负边距 | 部门成员表格底部间距不可控 | 删除 `app-vxe.css` 中的全局自动规则；页面如需间距用 Tailwind |

### 4.3 项目管理模块（Project）

| 文件路径 | 旧类 / 硬编码 | 具体问题 | 修复要求 |
|---------|-------------|---------|---------|
| `project/projects/index.vue` | `app-vxe-card-grid--flush` | 项目清单分页器贴底 | 移除 `--flush` |
| `project/my-related/index.vue` | `app-vxe-card-grid--flush` ×4 | 4 个 Grid 都贴底 | 移除 4 处 `--flush`；`.my-related { gap: 16px }` 改为 `gap-4`；`margin: 6px 0`、`padding: 12px` 改为 Tailwind |
| `project/pm-dashboard/index.vue` | `app-vxe-card-grid--flush` ×2 | 项目网格 + 行动队列贴底 | 移除 2 处 `--flush`；删除 `:deep(.ant-card-body) { padding-top: 12px }`；`padding: 10px 12px` 改为 Tailwind |
| `project/pmo-overview/index.vue` | `app-vxe-card-grid--flush` ×2 | 项目网格 + 行动队列贴底 | 移除 2 处 `--flush`；删除 `:deep(.ant-card-body) { padding-top: 12px }`；`padding: 10px 12px` 改为 Tailwind |
| `project/tasks/index.vue` | 未用旧类 | 可能受全局自动规则影响 | 删除 `app-vxe.css` 全局自动规则后确认 |
| `project/initiations/index.vue` | 未用旧类 | 可能受全局自动规则影响 | 删除 `app-vxe.css` 全局自动规则后确认 |

### 4.4 项目详情 Tab 页

| 文件路径 | 旧类 / 硬编码 | 具体问题 | 修复要求 |
|---------|-------------|---------|---------|
| `project/detail/components/ProjectPlanTab.vue` | `app-vxe-card-grid--section` | 里程碑表格底部贴底 | 移除 `--section`；依赖 `.project-detail__section-head { margin-bottom: 12px }` 改为 `mb-3`（后续统一） |
| `project/detail/components/ProjectIssuesTab.vue` | `app-vxe-card-grid--section` | 问题表格底部贴底 | 移除 `--section` |
| `project/detail/components/ProjectDeliveryTab.vue` | `app-vxe-card-grid--section` ×2 | 验收文件表格、交付资料表格贴底 | 移除 2 处 `--section` |
| `project/detail/components/ProjectDocsTab.vue` | `app-vxe-card-grid--section` ×2 | 文件资料表格、报告记录表格贴底 | 移除 2 处 `--section` |
| `project/detail/components/ProjectResourcesTab.vue` | `app-vxe-card-grid--section` ×2 | 项目成员表格、工时记录表格贴底 | 移除 2 处 `--section` |
| `project/detail/components/ProjectTasksTab.vue` | `app-vxe-card-grid--embedded` | 需求与排期 Grid 顶部微调 | 移除 `--embedded`；如需顶部间距用 `mt-2` / `mt-4` |
| `project/detail/components/ProjectWbsPanel.vue` | `app-vxe-card-grid--embedded` | WBS 面板 Grid 顶部微调 | 移除 `--embedded`；如需顶部间距用 `mt-2` / `mt-4` |
| `project/detail/index.vue` | `.project-detail__wbs-panel--embedded { margin: 8px 0 }` | 硬编码 margin | 改为 `my-2` |

### 4.5 集成中心模块（Integration）—— 全部需要修改

| 文件路径 | 旧类 / 硬编码 | 具体问题 | 修复要求 |
|---------|-------------|---------|---------|
| `integration/connectors/index.vue` | `app-vxe-card-grid--flush` | 连接器表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px/16px` 改为 `mb-3/mb-4` |
| `integration/sync-jobs/index.vue` | `app-vxe-card-grid--flush` ×2 | 同步任务表格、运行记录表格贴底 | 移除 2 处 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px/8px` 改为 Tailwind |
| `integration/webhook-events/index.vue` | `app-vxe-card-grid--flush` | Webhook 事件表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px/8px` 改为 Tailwind |
| `integration/groups/index.vue` | `app-vxe-card-grid--flush` | 分组表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px` 改为 `mb-3` |
| `integration/rules/index.vue` | `app-vxe-card-grid--flush` | 规则表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px` 改为 `mb-3` |
| `integration/templates/index.vue` | `app-vxe-card-grid--flush` | 模板表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px/8px` 改为 Tailwind |
| `integration/bindings/index.vue` | `app-vxe-card-grid--flush` ×5 | 5 个表格全部贴底 | 移除 5 处 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；删除 `:deep(.ant-card-body) { padding-top: 12px }`；`margin-bottom: 12px` 改为 `mb-3` |
| `integration/delivery-records/index.vue` | `app-vxe-card-grid--flush` | 交付记录表格贴底 | 移除 `--flush`；所有 Card 添加 `:bordered="false" class="rounded-2xl"`；`margin-bottom: 12px/8px` 改为 Tailwind |

### 4.6 AI 模块

| 文件路径 | 旧类 / 硬编码 | 具体问题 | 修复要求 |
|---------|-------------|---------|---------|
| `ai/providers/index.vue` | `app-vxe-card-grid--flush` ×2 | 供应商配置表格、模型配置表格贴底 | 移除 2 处 `--flush`；`.ai-provider-page { gap: 16px }` 改为 `gap-4`；`margin-bottom: 12px` 改为 `mb-3`；`padding: 12px` 改为 `p-3` |
| `ai/jobs/index.vue` | 硬编码 `.ai-jobs { gap: 16px }`、`.ai-jobs__toolbar { margin-bottom: 12px }` | 未用 Tailwind | `gap-4`、`mb-3`；Card `:bordered="false"` 统一 |
| `ai/writebacks/index.vue` | 硬编码 `.ai-writebacks { gap: 16px }`、`.ai-writebacks__filters { margin-bottom: 14px }` | 14px 是不标准值 | `gap-4`、`mb-4`（统一到 16px） |
| `ai/usage/index.vue` | 硬编码 `.ai-usage { gap: 16px }`、`.ai-usage__filters { margin-bottom: 12px }` | 未用 Tailwind | `gap-4`、`mb-3` |
| `ai/memory/index.vue` | 硬编码 `.ai-memory-page { gap: 16px }`、`.ai-memory-page__toolbar { margin-bottom: 12px }` | 未用 Tailwind | `gap-4`、`mb-3` |
| `ai/mcp/index.vue` | 硬编码 `.ai-mcp-page { gap: 16px }`、`.ai-mcp-page__toolbar { margin-bottom: 12px }` | 未用 Tailwind | `gap-4`、`mb-3` |
| `ai/prompts/index.vue` | 硬编码 `.ai-prompts-page { gap: 16px }`、多个 `margin-top: 12px/16px` | 间距方向不统一 | 统一用 `gap-4`、`mt-3`/`mt-4` |
| `ai/conversations/index.vue` | 大量 `padding`、`margin`、`border-radius` 硬编码 | 自定义聊天布局，需单独评估 | 按 Tailwind 逐步替换 |
| `ai/conversation-management/index.vue` | 硬编码 `.ai-session-management { gap: 12px }`、`padding: 14px 0` | 未用 Tailwind | `gap-3`、列表项用 `py-3.5` |

### 4.7 其他需要清理的硬编码样式

| 文件路径 | 硬编码内容 | 修复要求 |
|---------|-----------|---------|
| `project/projects/index.vue` | `.project-form__footer { margin: 0 -24px; padding: 16px 24px }` | 这是 Drawer footer，可保留，但确认是否需要 `-mx-6` |
| `project/detail/index.vue` | 大量 `.project-detail__*` 自定义 CSS | 项目详情页面未应用 Tailwind，需作为单独大工程处理 |
| `platform/dicts/index.vue` | `padding: 24px 0` | 改为 `py-6` |
| `platform/audit-logs/index.vue` | `.platform-audit__metrics :deep(.ant-card-body) { padding: 16px }` | 改为 Card body 默认 padding 或 `p-4` |

---

## 五、分阶段修复建议

### 第一阶段：止血（必须先做）

目标：让所有 VXE Grid 页面间距一致，不再贴底。

1. 修改 `app-vxe.css`：
   - 取消 `.app-vxe-card-grid--flush` 的底部负边距。
   - 删除 `.ant-card-body > .vxe-grid:first-child:last-child` 等全局自动规则。
   - 或统一合并为无负边距的 `.app-vxe-card-grid`。

2. 所有使用 `--flush` / `--section` / `--embedded` 的页面，将类名统一改为 `app-vxe-card-grid`（或无类名）。

影响文件约 20+ 个，但修改量小（只改 class 字符串）。

### 第二阶段：Tailwind 化

目标：逐步移除所有硬编码 `scoped style`。

1. 按模块逐个清理：
   - Integration 模块 8 个文件
   - AI 模块 9 个文件
   - Platform 模块问题文件
   - Project 模块问题文件

2. 统一 Card 属性：
   - 所有业务 Card 添加 `:bordered="false" class="rounded-2xl"`。
   - metrics Card 保持 `:bordered="false" class="rounded-2xl"`。

### 第三阶段：项目详情大改造

目标：把项目详情页从自定义 CSS 全面迁移到 Tailwind。

- 文件：`project/detail/index.vue` + `project/detail/components/*.vue`
- 这是最大工程，因为页面复杂、自定义类多。建议放在最后单独做。

---

## 六、验收标准

修复完成后，每个页面必须满足：

1. 不含有 `app-vxe-card-grid--flush`、`app-vxe-card-grid--section`、`app-vxe-card-grid--embedded`。
2. 不含有硬编码的 `margin`、`padding`、`gap`、`border-radius`（除 Drawer footer 等特殊场景）。
3. Card 统一使用 `:bordered="false" class="rounded-2xl"`。
4. 有分页表格：分页器与表格最后一行间距约 12px，与 Card 底部间距约 24px。
5. 无分页表格：表格最后一行与 Card 底部间距约 24px。
6. 同一模块内不同页面视觉一致。

---

## 七、需要人工确认的问题

1. **项目详情页是否要一次性 Tailwind 化？** 还是先做间距修复，Tailwind 化放在第二阶段？
2. **Card 圆角统一为 `rounded-2xl`（16px）还是 `rounded-xl`（12px）？** 目前项目里有 12px、20px 混用。
3. **metrics Card 是否也需要 `rounded-2xl`？** 目前部分页面 metrics 和列表 Card 圆角不一致。
4. **AI 的 conversations 页面是自定义聊天布局，是否按标准 Card+Grid 处理？** 还是单独保留自定义样式？
5. **是否允许保留少量 `scoped style` 用于复杂布局（如 grid-template-columns）？** 还是连布局都改用 Tailwind 的 `grid-cols-*`？

---

## 八、执行入口

如果你要按此清单自己改，建议顺序：

1. 先改 `app-vxe.css`（全局止血）。
2. 改所有 `--flush/section/embedded` 类名为 `app-vxe-card-grid` 或无类名。
3. 改 Integration 模块（问题最集中）。
4. 改 AI 模块。
5. 改 Platform 和 Project 模块。
6. 最后处理项目详情页的 Tailwind 化。

如果你希望我直接执行，我可以按第一阶段先全局止血，然后再逐步 Tailwind 化。
