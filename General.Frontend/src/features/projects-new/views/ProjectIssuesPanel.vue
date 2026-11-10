<script setup lang="ts">
import { computed, ref } from 'vue';
import { deleteProjectIssueApi, type ProjectApi } from '@/api/core/project';
import { UiAlertDialog, UiButton, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { hasAccessCode } from '@/session/access-store';
import ProjectIssueEditorDialog from './ProjectIssueEditorDialog.vue';
import ProjectRaidRiskDialog from './ProjectRaidRiskDialog.vue';
import ProjectRequirementEditorDialog from './ProjectRequirementEditorDialog.vue';

type IssueSectionTab = 'issues' | 'raid' | 'requirements';

const props = defineProps<{
  detail: ProjectApi.Detail;
  projectId: string;
}>();

const emit = defineEmits<{ changed: [] }>();

const activeTab = ref<IssueSectionTab>('issues');
const issueEditorOpen = ref(false);
const requirementEditorOpen = ref(false);
const raidEditorOpen = ref(false);
const selectedIssue = ref<ProjectApi.IssueItem | null>(null);
const selectedRequirement = ref<ProjectApi.RequirementItem | null>(null);
const selectedRaidItem = ref<ProjectApi.RaidItem | null>(null);
const deleteTarget = ref<ProjectApi.IssueItem | null>(null);
const deleteError = ref('');
const deleting = ref(false);

const canManageIssues = computed(() => hasAccessCode('Project.Quality.Manage'));
const canManageRaid = computed(() => hasAccessCode('Project.Raid.Manage'));
const canManageRequirements = computed(() => hasAccessCode('Project.Task.Manage'));
const issues = computed(() => props.detail.issues);
const raidItems = computed(() => props.detail.raidItems);
const requirements = computed(() => props.detail.requirements);

function openIssueEditor(issue: null | ProjectApi.IssueItem = null) {
  selectedIssue.value = issue;
  issueEditorOpen.value = true;
}

function openRequirementEditor(requirement: null | ProjectApi.RequirementItem = null) {
  selectedRequirement.value = requirement;
  requirementEditorOpen.value = true;
}

function openRaidEditor(item: ProjectApi.RaidItem) {
  selectedRaidItem.value = item;
  raidEditorOpen.value = true;
}

function requestDeleteIssue(issue: ProjectApi.IssueItem) {
  deleteError.value = '';
  deleteTarget.value = issue;
}

function updateDeleteOpen(open: boolean) {
  if (deleting.value) return;
  if (!open) {
    queueMicrotask(() => {
      if (deleting.value) return;
      deleteTarget.value = null;
      deleteError.value = '';
    });
  }
}

async function deleteIssue() {
  if (!deleteTarget.value || deleting.value) return;
  deleting.value = true;
  deleteError.value = '';
  try {
    await deleteProjectIssueApi(deleteTarget.value.id);
    deleteTarget.value = null;
    emit('changed');
  } catch (cause) {
    deleteError.value = cause instanceof Error ? cause.message : '问题删除失败，请重试。';
  } finally {
    deleting.value = false;
  }
}

function notifySaved() {
  emit('changed');
}

function setActiveTab(value: string) {
  if (value === 'issues' || value === 'raid' || value === 'requirements') activeTab.value = value;
}

function formatDate(value: null | string): string {
  return value?.slice(0, 10) ?? '—';
}

function displayName(value: null | string, fallback = '未提供姓名'): string {
  return value && value !== '-' && value !== '—' ? value : fallback;
}
</script>

<template>
  <section
    :class="$style.panel"
    aria-label="项目问题、RAID与需求"
  >
    <header :class="$style.header">
      <div>
        <h3 :class="$style.heading">问题、RAID 与需求</h3>
        <p :class="$style.description">记录按当前项目详情和数据范围读取。RAID只更新已有风险评估，需求没有删除接口。</p>
      </div>
      <UiButton
        v-if="activeTab === 'issues' && canManageIssues"
        variant="primary"
        size="sm"
        @click="openIssueEditor()"
      >
        新增问题
      </UiButton>
      <UiButton
        v-else-if="activeTab === 'requirements' && canManageRequirements"
        variant="primary"
        size="sm"
        @click="openRequirementEditor()"
      >
        新增需求
      </UiButton>
    </header>

    <UiTabs
      :model-value="activeTab"
      @update:model-value="setActiveTab"
    >
      <UiTabsList label="项目问题、RAID和需求">
        <UiTabsTrigger value="issues">问题（{{ issues.length }}）</UiTabsTrigger>
        <UiTabsTrigger value="raid">RAID（{{ raidItems.length }}）</UiTabsTrigger>
        <UiTabsTrigger value="requirements">需求（{{ requirements.length }}）</UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div
      v-if="activeTab === 'issues' && issues.length"
      :class="$style.list"
    >
      <article
        v-for="issue in issues"
        :key="issue.id"
        :class="$style.item"
      >
        <div :class="$style.itemMain">
          <div :class="$style.titleRow">
            <h4 :class="$style.itemTitle">{{ issue.title }}</h4>
            <span :class="$style.status">{{ issue.status }}</span>
          </div>
          <p :class="$style.meta">
            {{ issue.type }} · {{ issue.level }} · 负责人 {{ displayName(issue.ownerName) }} · 到期
            {{ formatDate(issue.dueDate) }}
            <span v-if="issue.isOverdue"> · 已逾期</span>
          </p>
          <p :class="$style.meta">
            需求：{{ issue.requirementTitle || '未关联' }} · 验收标准：{{
              issue.acceptanceCriteriaId ? displayName(issue.acceptanceCriteriaTitle) : '未关联'
            }}
            · 质量检查单：{{ issue.qualityChecklistId ? displayName(issue.qualityChecklistTitle) : '未关联' }}
          </p>
          <p
            v-if="issue.productOwnerName || issue.developerOwnerName || issue.testerOwnerName"
            :class="$style.meta"
          >
            产品：{{ issue.productOwnerUserId ? displayName(issue.productOwnerName) : '—' }} · 开发：{{
              issue.developerOwnerUserId ? displayName(issue.developerOwnerName) : '—'
            }}
            · 测试：{{ issue.testerOwnerUserId ? displayName(issue.testerOwnerName) : '—' }}
          </p>
        </div>
        <div
          v-if="canManageIssues"
          :class="$style.actions"
        >
          <UiButton
            variant="secondary"
            size="sm"
            @click="openIssueEditor(issue)"
          >
            编辑
          </UiButton>
          <UiButton
            variant="danger"
            size="sm"
            @click="requestDeleteIssue(issue)"
          >
            删除
          </UiButton>
        </div>
      </article>
    </div>
    <p
      v-else-if="activeTab === 'issues'"
      :class="$style.empty"
    >
      当前项目没有可见的问题记录。
    </p>

    <div
      v-if="activeTab === 'raid' && raidItems.length"
      :class="$style.list"
    >
      <article
        v-for="item in raidItems"
        :key="item.id"
        :class="$style.item"
      >
        <div :class="$style.itemMain">
          <div :class="$style.titleRow">
            <h4 :class="$style.itemTitle">{{ item.title }}</h4>
            <span :class="$style.status">{{ item.status }}</span>
          </div>
          <p :class="$style.meta">
            {{ item.type }} · {{ item.level }} · 负责人 {{ displayName(item.ownerName) }} · 计划解决
            {{ formatDate(item.plannedResolveDate) }}
            <span v-if="item.isOverdue"> · 已逾期</span>
          </p>
          <p :class="$style.meta">
            后端风险分 {{ item.riskScore }} · 概率 {{ item.probability }} · 影响 {{ item.impact }}
          </p>
          <p
            v-if="item.responseStrategy || item.triggerCondition || item.contingencyPlan"
            :class="$style.meta"
          >
            应对：{{ item.responseStrategy || '—' }} · 触发：{{ item.triggerCondition || '—' }} · 应急：{{
              item.contingencyPlan || '—'
            }}
          </p>
        </div>
        <div
          v-if="canManageRaid && item.type === '风险'"
          :class="$style.actions"
        >
          <UiButton
            variant="secondary"
            size="sm"
            @click="openRaidEditor(item)"
          >
            更新风险评估
          </UiButton>
        </div>
      </article>
    </div>
    <p
      v-else-if="activeTab === 'raid'"
      :class="$style.empty"
    >
      当前项目没有可见的 RAID 记录。
    </p>
    <p
      v-if="activeTab === 'raid' && raidItems.some((item) => item.type !== '风险')"
      :class="$style.note"
    >
      当前后端仅提供“风险”类型的风险评估更新；其它 RAID 类型按只读记录展示。
    </p>

    <div
      v-if="activeTab === 'requirements' && requirements.length"
      :class="$style.list"
    >
      <article
        v-for="requirement in requirements"
        :key="requirement.id"
        :class="$style.item"
      >
        <div :class="$style.itemMain">
          <div :class="$style.titleRow">
            <p :class="$style.code">{{ requirement.requirementCode }} · V{{ requirement.versionNo }}</p>
            <h4 :class="$style.itemTitle">{{ requirement.title }}</h4>
            <span :class="$style.status">{{ requirement.status }}</span>
            <span
              v-if="requirement.isCurrent"
              :class="$style.current"
              >当前版本</span
            >
          </div>
          <p :class="$style.meta">
            {{ requirement.type }} · {{ requirement.priority }} · 来源 {{ requirement.source }} · 估算
            {{ requirement.sizeEstimate ?? '—' }}
          </p>
          <p :class="$style.meta">
            WBS：{{ requirement.wbsNodeName || '未关联' }} · 里程碑：{{ requirement.milestoneName || '未关联' }} ·
            替代：{{ requirement.replacesRequirementTitle || '无' }}
          </p>
          <p :class="$style.meta">
            产品：{{ requirement.productOwnerUserId ? displayName(requirement.productOwnerName) : '未指定' }} · 业务：{{
              requirement.businessOwnerUserId ? displayName(requirement.businessOwnerName) : '未指定'
            }}
          </p>
          <p
            v-if="requirement.description"
            :class="$style.summary"
          >
            {{ requirement.description }}
          </p>
        </div>
        <div
          v-if="canManageRequirements"
          :class="$style.actions"
        >
          <UiButton
            variant="secondary"
            size="sm"
            @click="openRequirementEditor(requirement)"
          >
            编辑
          </UiButton>
        </div>
      </article>
    </div>
    <p
      v-else-if="activeTab === 'requirements'"
      :class="$style.empty"
    >
      当前项目没有可见的需求记录。
    </p>

    <ProjectIssueEditorDialog
      v-model:open="issueEditorOpen"
      :detail="detail"
      :issue="selectedIssue"
      :project-id="projectId"
      @saved="notifySaved"
    />
    <ProjectRequirementEditorDialog
      v-model:open="requirementEditorOpen"
      :detail="detail"
      :project-id="projectId"
      :requirement="selectedRequirement"
      @saved="notifySaved"
    />
    <ProjectRaidRiskDialog
      v-model:open="raidEditorOpen"
      :item="selectedRaidItem"
      @saved="notifySaved"
    />
    <UiAlertDialog
      :open="Boolean(deleteTarget)"
      title="删除项目问题？"
      :description="deleteTarget ? `确定删除“${deleteTarget.title}”吗？该操作会直接移除问题记录。` : ''"
      confirm-label="删除问题"
      @update:open="updateDeleteOpen"
      @confirm="deleteIssue"
    >
      <p
        v-if="deleteError"
        role="alert"
        :class="$style.error"
      >
        {{ deleteError }}
      </p>
      <p
        v-if="deleting"
        role="status"
      >
        正在删除问题…
      </p>
    </UiAlertDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.panel {
  @apply min-w-0 space-y-3 border-t pt-4;
}

.header {
  @apply flex items-start justify-between gap-3;
}

.heading {
  @apply text-[13px] font-semibold;
  color: #172b4d;
}

.description {
  @apply mt-1 text-[10px] leading-4;
  color: #6b778c;
}

.list {
  @apply space-y-2;
}

.item {
  @apply flex items-start justify-between gap-3 rounded border bg-white p-3;
  border-color: #dfe1e6;
}

.itemMain {
  @apply min-w-0 flex-1;
}

.titleRow {
  @apply flex flex-wrap items-center gap-2;
}

.itemTitle {
  @apply text-[12px] font-semibold;
  color: #172b4d;
}

.code,
.meta,
.summary,
.note {
  @apply text-[10px] leading-4;
  color: #6b778c;
}

.code {
  @apply font-semibold;
  color: #0052cc;
}

.meta,
.summary {
  @apply mt-1;
}

.status,
.current {
  @apply rounded px-2 py-0.5 text-[10px];
  color: #42526e;
  background: #f4f5f7;
}

.current {
  color: #006644;
  background: #e3fcef;
}

.actions {
  @apply flex shrink-0 flex-wrap gap-2;
}

.empty {
  @apply rounded border border-dashed px-3 py-5 text-center text-[11px];
  border-color: #dfe1e6;
  color: #6b778c;
}

.note {
  @apply rounded border px-3 py-2;
  border-color: #dfe1e6;
}

.error {
  @apply text-[11px];
  color: var(--ui-danger);
}

@media (max-width: 720px) {
  .header,
  .item {
    @apply flex-col;
  }

  .actions {
    @apply w-full justify-end;
  }
}
</style>
