<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import type { BusinessApi } from '@/api/core/business';
import { getBusinessProjectDetailApi } from '@/api/core/business';
import type { ProjectApi } from '@/api/core/project';
import { getProjectDetailSectionApi } from '@/api/core/project';
import { UiButton, UiDialog, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import BusinessProjectRecordsPanel from '@/features/business/views/BusinessProjectRecordsPanel.vue';
import { sessionState } from '@/session/session-store';
import { hasAccessCode } from '@/session/access-store';
import {
  buildBusinessDetailRows,
  buildBusinessDetailSummaryRows,
  type BusinessDetailViewRow,
} from '@/features/business/model/business.models';
import ProjectGovernancePanel from './ProjectGovernancePanel.vue';
import ProjectIssuesPanel from './ProjectIssuesPanel.vue';
import ProjectMeetingRecordDialog from './ProjectMeetingRecordDialog.vue';
import ProjectPlanningPanel from './ProjectPlanningPanel.vue';
import ProjectQualityPanel from './ProjectQualityPanel.vue';
import ProjectResourcesPanel from './ProjectResourcesPanel.vue';
import ProjectTasksPanel from './ProjectTasksPanel.vue';

interface DetailViewRow {
  id: string;
  label: string;
  primary: string;
  secondary: string;
  status?: string;
}

const props = defineProps<{
  businessOnly?: boolean;
  open: boolean;
  projectId: string;
  projectName: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  changed: [];
}>();

const sections: Array<{ key: ProjectApi.DetailSection; label: string }> = [
  { key: 'overview', label: '项目概况' },
  { key: 'plan', label: '计划' },
  { key: 'tasks', label: '任务' },
  { key: 'issues', label: '问题与风险' },
  { key: 'control', label: '项目控制' },
  { key: 'delivery', label: '验收交付' },
  { key: 'docs', label: '文档与会议' },
  { key: 'resources', label: '成员与资源' },
];

const activeSection = ref<ProjectApi.DetailSection>('overview');
const detail = ref<ProjectApi.Detail | null>(null);
const businessDetail = ref<BusinessApi.ProjectDetail | null>(null);
const loading = ref(false);
const error = ref('');
const businessLoading = ref(false);
const businessError = ref('');
const meetingDialogOpen = ref(false);
const canManageReports = computed(() => hasAccessCode('Project.Report.Manage'));
const sectionCache = new Map<ProjectApi.DetailSection, ProjectApi.Detail>();
let activeRequest: AbortController | null = null;
let businessActiveRequest: AbortController | null = null;
let requestVersion = 0;
let businessRequestVersion = 0;

const businessSummaryRows = computed(() =>
  businessDetail.value ? buildBusinessDetailSummaryRows(businessDetail.value.summary) : [],
);
const businessRows = computed<BusinessDetailViewRow[]>(() =>
  businessDetail.value ? buildBusinessDetailRows(businessDetail.value) : [],
);
const businessForecastRows = computed(() => businessRows.value.filter((row) => row.id.startsWith('forecast-')));

const overviewRows = computed(() => {
  if (!detail.value) return [];
  const value = detail.value;
  return [
    { label: '项目编号', value: value.projectCode },
    { label: '项目名称', value: value.name },
    { label: '状态', value: value.status },
    { label: '优先级', value: value.priority },
    { label: '所属组织', value: value.organizationUnitName },
    { label: '项目经理', value: value.managerName },
    { label: '发起人', value: value.sponsorName },
    { label: '开始日期', value: formatDate(value.plannedStartDate) },
    { label: '计划结束', value: formatDate(value.plannedEndDate) },
    { label: '成员数', value: String(value.memberCount) },
    { label: '我的关系', value: value.myRelation },
    { label: '健康状态', value: value.healthStatus },
    { label: '健康风险分', value: String(value.healthRiskScore) },
    { label: '任务完成率', value: `${value.taskCompletionRate}%` },
    { label: '开放问题', value: String(value.openIssueCount) },
    { label: '健康数据时间', value: formatDate(value.healthCalculatedTime) },
  ];
});

const sectionRows = computed<DetailViewRow[]>(() => {
  if (!detail.value) return [];
  const value = detail.value;
  switch (activeSection.value) {
    case 'plan':
      return [
        countRow('wbs', 'WBS 节点', value.wbsNodes.length),
        countRow('sprints', 'Sprint', value.sprints.length),
        ...value.milestones.map((item) => ({
          id: `milestone-${item.id}`,
          label: '里程碑',
          primary: item.name,
          secondary: `${item.ownerName} · 计划完成 ${formatDate(item.plannedCompletionDate)}`,
          status: item.status,
        })),
        countRow('requirements', '需求', value.requirements.length),
      ];
    case 'tasks':
      return [
        countRow('dependencies', '任务依赖', value.taskDependencies.length),
        countRow('work-hours', '累计工时', value.totalWorklogHours, '小时'),
        ...value.tasks.map((item) => ({
          id: `task-${item.id}`,
          label: item.taskCode,
          primary: item.title,
          secondary: `${item.projectName} · ${item.ownerName} · 截止 ${formatDate(item.plannedEndTime)}`,
          status: item.status,
        })),
      ];
    case 'control':
      return [
        countRow('changes', '变更请求', value.changeRequests.length),
        {
          id: 'baseline',
          label: '当前基线',
          primary: value.activeBaseline ? '已建立' : '未建立',
          secondary: '状态来自项目详情接口。',
        },
        countRow('performance', '绩效快照', value.performanceSnapshots.length),
        { id: 'health', label: '健康状态', primary: value.healthStatus, secondary: value.healthDataSource },
        ...value.healthBreakdowns.map((item) => ({
          id: `health-${item.code}`,
          label: item.label,
          primary: item.reason,
          secondary: `${item.dataSource} · ${item.ruleVersion}`,
        })),
      ];
    case 'delivery':
      return [
        countRow('acceptance-criteria', '验收标准', value.acceptanceCriteria.length),
        countRow('acceptance-records', '验收记录', value.acceptanceRecords.length),
      ];
    case 'docs':
      return [
        countRow('reports', '项目报告', value.reports.length),
        ...value.reports.map((item) => ({
          id: `report-${item.id}`,
          label: `${item.reportType} · ${formatDate(item.periodStart)} 至 ${formatDate(item.periodEnd)}`,
          primary: item.title,
          secondary: `${item.authorName} · ${item.attachmentFileName || '无附件'}`,
          status: item.status,
        })),
        countRow('meetings', '会议记录', value.meetingRecords.length),
        ...value.meetingRecords.map((item) => ({
          id: `meeting-${item.id}`,
          label: `会议 · ${formatDateTime(item.meetingTime)}`,
          primary: item.topic,
          secondary: `${item.facilitatorName} · ${item.participantNames.join('、') || '未记录参会人'} · ${item.attachmentFileName ? `附件 ${item.attachmentFileName}` : '无附件'} · 行动项 ${item.actionItems.length} 项`,
          status: item.status,
        })),
        ...value.documents.map((item) => ({
          id: `document-${item.id}`,
          label: `${item.category} · ${item.version}`,
          primary: item.title,
          secondary: `${item.ownerName} · ${item.summary}`,
          status: item.status,
        })),
      ];
    case 'resources':
      return [
        countRow('calendar', '日历事项', value.calendarItems.length),
        countRow('worklogs', '工时记录', value.worklogs.length),
        countRow('work-hours', '累计工时', value.totalWorklogHours, '小时'),
        ...value.members.map((item) => ({
          id: `member-${item.id}`,
          label: item.accessLevel,
          primary: item.userName,
          secondary: `${item.organizationUnitName} · ${item.roleNames.join('、') || '未设置角色'}`,
          status: item.isActive ? '在岗' : '已离开',
        })),
      ];
    case 'overview':
      return value.healthBreakdowns.map((item) => ({
        id: `health-${item.code}`,
        label: item.label,
        primary: item.reason,
        secondary: `${item.dataSource} · ${item.ruleVersion}`,
      }));
  }
  return [];
});

function formatDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}

function formatDateTime(value: string): string {
  return value ? value.slice(0, 16).replace('T', ' ') : '—';
}

function countRow(id: string, label: string, count: number, unit = '项'): DetailViewRow {
  return { id, label, primary: `${count} ${unit}`, secondary: '数量来自项目详情接口。' };
}

async function loadSection(section: ProjectApi.DetailSection) {
  if (!props.projectId) return;
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentRequest = ++requestVersion;
  activeSection.value = section;
  loading.value = true;
  error.value = '';
  detail.value = null;

  try {
    const loaded = await getProjectDetailSectionApi(section, props.projectId, controller.signal);
    if (currentRequest !== requestVersion) return;
    detail.value = loaded;
    sectionCache.set(section, loaded);
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== requestVersion) return;
    error.value = cause instanceof Error ? cause.message : '项目详情读取失败，请稍后重试。';
  } finally {
    if (currentRequest === requestVersion) loading.value = false;
  }
}

async function loadBusinessDetail() {
  if (!props.projectId) return;
  businessActiveRequest?.abort();
  const controller = new AbortController();
  businessActiveRequest = controller;
  const currentRequest = ++businessRequestVersion;
  businessLoading.value = true;
  businessError.value = '';
  businessDetail.value = null;

  try {
    businessDetail.value = await getBusinessProjectDetailApi(props.projectId, controller.signal);
  } catch (cause) {
    if (controller.signal.aborted || currentRequest !== businessRequestVersion) return;
    businessError.value = cause instanceof Error ? cause.message : '项目经营详情读取失败，请稍后重试。';
  } finally {
    if (currentRequest === businessRequestVersion) businessLoading.value = false;
  }
}

async function reloadBusinessDetail() {
  await loadBusinessDetail();
  emit('changed');
}

function selectSection(value: string) {
  const section = sections.find((item) => item.key === value)?.key;
  if (!section) return;
  const cached = sectionCache.get(section);
  if (cached) {
    activeSection.value = section;
    detail.value = cached;
    return;
  }
  void loadSection(section);
}

watch(
  () => [props.open, props.projectId, props.businessOnly] as const,
  ([isOpen, projectId, businessOnly]) => {
    activeRequest?.abort();
    businessActiveRequest?.abort();
    requestVersion += 1;
    businessRequestVersion += 1;
    detail.value = null;
    businessDetail.value = null;
    error.value = '';
    businessError.value = '';
    loading.value = false;
    businessLoading.value = false;
    sectionCache.clear();
    activeSection.value = 'overview';
    if (isOpen && projectId) {
      if (businessOnly) {
        void loadBusinessDetail();
      } else {
        void loadSection('overview');
      }
    }
  },
  { immediate: true },
);

onBeforeUnmount(() => {
  activeRequest?.abort();
  businessActiveRequest?.abort();
});

function closeDialog() {
  emit('update:open', false);
}

function reloadGovernance() {
  void loadSection('control');
  emit('changed');
}

function reloadPlanning() {
  void loadSection('plan');
  emit('changed');
}

function reloadTasks() {
  void loadSection('tasks');
  emit('changed');
}

function reloadResources() {
  void loadSection('resources');
  emit('changed');
}

function reloadDelivery() {
  void loadSection('delivery');
  emit('changed');
}

function reloadDocs() {
  void loadSection('docs');
  emit('changed');
}

function reloadIssues() {
  void loadSection('issues');
  emit('changed');
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="projectName || (businessOnly ? '项目经营详情' : '项目详情')"
    :description="
      businessOnly
        ? '经营数据按当前账号范围读取；汇总指标和敏感金额口径由后端提供。'
        : '详情内容按当前项目访问范围读取；健康和进度口径由后端提供。'
    "
    size="xl"
    @update:open="emit('update:open', $event)"
  >
    <div v-if="businessOnly">
      <div
        v-if="businessLoading"
        role="status"
        aria-live="polite"
      >
        正在读取项目经营详情…
      </div>
      <div
        v-else-if="businessError"
        role="alert"
      >
        <span>{{ businessError }}</span>
        <UiButton
          variant="secondary"
          size="sm"
          @click="loadBusinessDetail"
        >
          重试
        </UiButton>
      </div>
      <div v-else-if="businessDetail">
        <UiTabs model-value="business">
          <UiTabsList label="项目经营详情分区">
            <UiTabsTrigger value="business">经营汇总</UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
        <dl>
          <div
            v-for="row in businessSummaryRows"
            :key="row.label"
          >
            <dt>{{ row.label }}</dt>
            <dd>{{ row.value || '—' }}</dd>
          </div>
        </dl>
        <p v-if="!businessDetail.canViewSensitiveAmounts">敏感金额按当前权限由服务端脱敏；未返回的金额显示为“—”。</p>
        <BusinessProjectRecordsPanel
          :current-user-id="sessionState.user?.id ?? ''"
          :current-user-name="sessionState.user?.realName || sessionState.user?.username || '当前用户'"
          :detail="businessDetail"
          :project-id="projectId"
          @changed="reloadBusinessDetail"
        />
        <section
          v-if="businessForecastRows.length"
          aria-label="经营预测历史"
        >
          <h3>经营预测历史</h3>
          <article
            v-for="row in businessForecastRows"
            :key="row.id"
          >
            <div>
              <p>{{ row.label }}</p>
              <p>{{ row.primary || '—' }}</p>
              <p>{{ row.secondary || '—' }}</p>
            </div>
            <span v-if="row.status">{{ row.status }}</span>
          </article>
        </section>
      </div>
    </div>
    <div
      v-else-if="loading"
      role="status"
      aria-live="polite"
    >
      正在读取项目详情…
    </div>
    <div
      v-else-if="error"
      role="alert"
    >
      <span>{{ error }}</span>
      <UiButton
        variant="secondary"
        size="sm"
        @click="loadSection(activeSection)"
      >
        重试
      </UiButton>
    </div>
    <div v-else-if="detail">
      <UiTabs
        :model-value="activeSection"
        @update:model-value="selectSection"
      >
        <UiTabsList label="项目详情分区">
          <UiTabsTrigger
            v-for="section in sections"
            :key="section.key"
            :value="section.key"
          >
            {{ section.label }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <dl v-if="activeSection === 'overview'">
        <div
          v-for="row in overviewRows"
          :key="row.label"
        >
          <dt>{{ row.label }}</dt>
          <dd>{{ row.value || '—' }}</dd>
        </div>
      </dl>

      <ProjectPlanningPanel
        v-if="activeSection === 'plan'"
        :detail="detail"
        :project-id="projectId"
        @changed="reloadPlanning"
      />

      <ProjectTasksPanel
        v-else-if="activeSection === 'tasks'"
        :detail="detail"
        :project-id="projectId"
        @changed="reloadTasks"
      />

      <ProjectIssuesPanel
        v-else-if="activeSection === 'issues'"
        :detail="detail"
        :project-id="projectId"
        @changed="reloadIssues"
      />

      <ProjectResourcesPanel
        v-else-if="activeSection === 'resources'"
        :detail="detail"
        @changed="reloadResources"
      />

      <ProjectQualityPanel
        v-if="activeSection === 'delivery'"
        :detail="detail"
        :project-id="projectId"
        @changed="reloadDelivery"
        @open-issues="loadSection('issues')"
      />

      <section
        v-else-if="activeSection !== 'overview'"
        :aria-label="sections.find((item) => item.key === activeSection)?.label"
      >
        <UiButton
          v-if="activeSection === 'docs' && canManageReports"
          variant="primary"
          size="sm"
          @click="meetingDialogOpen = true"
        >
          新增会议记录
        </UiButton>
        <article
          v-for="row in sectionRows"
          :key="row.id"
        >
          <div>
            <p>{{ row.label }}</p>
            <p>{{ row.primary }}</p>
            <p>{{ row.secondary }}</p>
          </div>
          <span v-if="row.status">
            {{ row.status }}
          </span>
        </article>
        <p v-if="sectionRows.length === 0">当前分区没有可见记录。</p>
      </section>

      <ProjectMeetingRecordDialog
        v-if="activeSection === 'docs'"
        v-model:open="meetingDialogOpen"
        :detail="detail"
        :project-id="projectId"
        @saved="reloadDocs"
      />

      <ProjectGovernancePanel
        v-if="activeSection === 'control'"
        :detail="detail"
        :project-id="projectId"
        @changed="reloadGovernance"
      />

      <section
        v-if="activeSection === 'overview' && sectionRows.length"
        aria-label="后端健康说明"
      >
        <article
          v-for="row in sectionRows"
          :key="row.id"
        >
          <p>{{ row.label }}</p>
          <p>{{ row.primary }}</p>
          <p>{{ row.secondary }}</p>
        </article>
      </section>
    </div>

    <template #footer>
      <UiButton
        variant="secondary"
        @click="closeDialog"
      >
        关闭
      </UiButton>
    </template>
  </UiDialog>
</template>
