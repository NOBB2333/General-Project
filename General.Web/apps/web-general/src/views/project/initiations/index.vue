<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  Space,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  approveProjectCharterApi,
  getProjectDetailApi,
  getProjectListApi,
  rejectProjectCharterApi,
  saveProjectCharterApi,
  submitProjectCharterApprovalApi,
  updateProjectStatusApi,
  type ProjectApi,
} from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectInitiationPage' });

const router = useRouter();
const loadingProjects = ref(false);
const loadingDetail = ref(false);
const savingCharter = ref(false);
const processingApproval = ref('');
const promotingProject = ref(false);
const items = ref<ProjectApi.ProjectListItem[]>([]);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const selectedProjectId = ref<string>();
const selectedDetail = ref<ProjectApi.ProjectDetail | null>(null);
const charterForm = reactive<ProjectApi.CharterSaveInput>({
  approvalStatus: '草稿',
  assumptions: '',
  businessCase: '',
  constraints: '',
  objectives: '',
  scopeBoundary: '',
  successCriteria: '',
});
const initiationDicts = useDictOptions(
  ['project_status', 'task_priority'],
  createDictOptionsFallback({
    project_status: ['待规划', '进行中', '已完成', '已关闭'],
    task_priority: ['P0', 'P1', 'P2', 'P3'],
  }),
);

const columns = [
  { field: 'projectCode', slots: { default: 'projectCode' }, title: '项目编号', width: 130 },
  { align: 'left', field: 'name', title: '项目名称', width: 180 },
  { field: 'managerName', title: '项目经理', width: 110 },
  { field: 'priority', slots: { default: 'priority' }, title: '优先级', width: 90 },
  { field: 'plannedEndDate', slots: { default: 'plannedEndDate' }, title: '目标完成', width: 120 },
] satisfies VxeGridProps<ProjectApi.ProjectListItem>['columns'];

const gridOptions = computed<VxeGridProps<ProjectApi.ProjectListItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loadingProjects.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const gridEvents: NativeVxeGridEvents<ProjectApi.ProjectListItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<ProjectApi.ProjectListItem>({
  gridEvents,
  gridOptions: {},
});

const selectedProject = computed(
  () => items.value.find((item) => item.id === selectedProjectId.value) ?? null,
);
const charterStatus = computed(
  () =>
    selectedDetail.value?.charter?.approvalStatus ?? charterForm.approvalStatus,
);
const approvalRecords = computed(
  () => selectedDetail.value?.charter?.approvalWorkflow?.approvalRecords ?? [],
);
const completionItems = computed(() => [
  { label: '商业论证', ready: Boolean(charterForm.businessCase.trim()) },
  { label: '项目目标', ready: Boolean(charterForm.objectives.trim()) },
  { label: '范围边界', ready: Boolean(charterForm.scopeBoundary.trim()) },
  { label: '成功标准', ready: Boolean(charterForm.successCriteria.trim()) },
  { label: '审批状态', ready: charterStatus.value === '已批准' },
]);
const completedRequiredCount = computed(
  () => completionItems.value.filter((item) => item.ready).length,
);
const canSaveCharter = computed(() => Boolean(selectedProjectId.value));
async function loadInitiations(projectId?: string) {
  loadingProjects.value = true;
  try {
    const planningStatus =
      initiationDicts
        .getOptions('project_status')
        .value.find((item) => item.value === '待规划')?.value ?? '待规划';
    items.value = await getProjectListApi({ status: planningStatus });
    const nextProjectId =
      projectId && items.value.some((item) => item.id === projectId)
        ? projectId
        : items.value[0]?.id;
    if (nextProjectId) {
      await selectProject(nextProjectId);
    } else {
      selectedProjectId.value = undefined;
      selectedDetail.value = null;
      resetCharterForm();
    }
  } catch (error) {
    showRequestError(error, '待立项项目加载失败');
  } finally {
    loadingProjects.value = false;
  }
}

async function selectProject(projectId: string) {
  selectedProjectId.value = projectId;
  await loadProjectDetail(projectId);
}

async function loadProjectDetail(projectId: string) {
  loadingDetail.value = true;
  try {
    const detail = await getProjectDetailApi(projectId);
    selectedDetail.value = detail;
    syncCharterForm(detail.charter);
  } catch (error) {
    showRequestError(error, '项目立项详情加载失败');
  } finally {
    loadingDetail.value = false;
  }
}

function syncCharterForm(charter?: null | ProjectApi.Charter) {
  charterForm.approvalStatus = charter?.approvalStatus || '草稿';
  charterForm.assumptions = charter?.assumptions || '';
  charterForm.businessCase = charter?.businessCase || '';
  charterForm.constraints = charter?.constraints || '';
  charterForm.objectives = charter?.objectives || '';
  charterForm.scopeBoundary = charter?.scopeBoundary || '';
  charterForm.successCriteria = charter?.successCriteria || '';
}

function resetCharterForm() {
  syncCharterForm(null);
}

async function handleSaveCharter() {
  if (!selectedProjectId.value || !canSaveCharter.value) {
    message.warning('请先选择项目');
    return false;
  }

  savingCharter.value = true;
  try {
    await saveProjectCharterApi(selectedProjectId.value, {
      assumptions: normalizeCharterSummary(charterForm.assumptions),
      businessCase: normalizeCharterSummary(charterForm.businessCase),
      constraints: normalizeCharterSummary(charterForm.constraints),
      objectives: normalizeCharterSummary(charterForm.objectives),
      scopeBoundary: normalizeCharterSummary(charterForm.scopeBoundary),
      successCriteria: normalizeCharterSummary(charterForm.successCriteria),
      approvalStatus: charterStatus.value === '退回' ? '退回' : '草稿',
    });
    message.success('项目章程已保存');
    await loadProjectDetail(selectedProjectId.value);
    return true;
  } catch (error) {
    showRequestError(error, '项目章程保存失败');
    return false;
  } finally {
    savingCharter.value = false;
  }
}

function normalizeCharterSummary(value?: null | string) {
  return value?.trim() || '';
}

async function handleSubmitApproval() {
  if (!selectedProjectId.value) {
    return;
  }

  const saved = await handleSaveCharter();
  if (!saved) {
    return;
  }

  processingApproval.value = 'submit';
  try {
    await submitProjectCharterApprovalApi(selectedProjectId.value, {
      comment: '立项页提交项目章程审批',
    });
    message.success('项目章程已提交审批');
    await loadProjectDetail(selectedProjectId.value);
  } catch (error) {
    showRequestError(error, '项目章程提交审批失败');
  } finally {
    processingApproval.value = '';
  }
}

async function handleApproveApproval() {
  if (!selectedProjectId.value) {
    return;
  }

  processingApproval.value = 'approve';
  try {
    await approveProjectCharterApi(selectedProjectId.value, {
      comment: '立项页同意项目章程',
    });
    message.success('项目章程已通过审批');
    await loadProjectDetail(selectedProjectId.value);
  } catch (error) {
    showRequestError(error, '项目章程审批失败');
  } finally {
    processingApproval.value = '';
  }
}

async function handleRejectApproval() {
  if (!selectedProjectId.value) {
    return;
  }

  processingApproval.value = 'reject';
  try {
    await rejectProjectCharterApi(selectedProjectId.value, {
      comment: '立项页退回项目章程',
    });
    message.success('项目章程已退回');
    await loadProjectDetail(selectedProjectId.value);
  } catch (error) {
    showRequestError(error, '项目章程退回失败');
  } finally {
    processingApproval.value = '';
  }
}

async function handlePromoteProject() {
  if (!selectedProjectId.value) {
    return;
  }

  if (charterStatus.value !== '已批准') {
    message.warning('项目章程审批通过后才能进入执行中');
    return;
  }

  promotingProject.value = true;
  try {
    await updateProjectStatusApi(selectedProjectId.value, { status: '进行中' });
    message.success('项目已进入执行中');
    await loadInitiations();
  } catch (error) {
    showRequestError(error, '项目状态推进失败');
  } finally {
    promotingProject.value = false;
  }
}

function openProjectDetail() {
  if (!selectedProjectId.value) {
    return;
  }

  localStorage.setItem('general:last-project-id', selectedProjectId.value);
  router.push({
    path: '/project/detail',
  });
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function resolveStatusColor(status?: string) {
  if (status === '已批准') {
    return 'success';
  }

  if (status === '退回' || status === '已驳回') {
    return 'error';
  }

  if (status === '待审批') {
    return 'processing';
  }

  return 'default';
}

function showRequestError(error: unknown, fallback: string) {
  const description = error instanceof Error ? error.message : fallback;
  message.error(description || fallback);
}

onMounted(async () => {
  await initiationDicts.load();
  await loadInitiations();
});
</script>

<template>
  <Page title="项目立项">
    <section class="project-initiation">
      <div class="project-initiation__summary">
        <div>
          <span class="project-initiation__eyebrow">启动过程组</span>
          <h2>项目章程与立项审批</h2>
        </div>
        <div class="project-initiation__metrics">
          <article>
            <span>待立项项目</span>
            <strong>{{ items.length }}</strong>
          </article>
          <article>
            <span>当前章程</span>
            <strong
              >{{ completedRequiredCount }}/{{ completionItems.length }}</strong
            >
          </article>
          <article>
            <span>审批状态</span>
            <strong>{{ charterStatus || '-' }}</strong>
          </article>
        </div>
      </div>

      <div class="project-initiation__layout">
        <Card
          :bordered="false"
          class="project-initiation__list rounded-2xl"
          title="待规划项目"
        >
          <div
            v-if="!loadingProjects && items.length === 0"
            class="project-initiation__empty"
          >
            <Empty description="暂无待规划项目" />
          </div>
          <Grid
            v-else
            :grid-events="gridEvents"
            :grid-options="gridOptions"
          >
            <template #projectCode="{ row }">
              <Button size="small" type="link" @click="selectProject(row.id)">
                {{ row.projectCode }}
              </Button>
            </template>
            <template #priority="{ row }">
              <Tag
                :color="
                  initiationDicts.getColor(
                    'task_priority',
                    row.priority,
                    'processing',
                  )
                "
              >
                {{ row.priority }}
              </Tag>
            </template>
            <template #plannedEndDate="{ row }">
              {{ formatDate(row.plannedEndDate) }}
            </template>
          </Grid>
        </Card>

        <Card :bordered="false" class="project-initiation__editor rounded-2xl">
          <template #title>
            <div class="project-initiation__selected-title">
              <span>立项详情</span>
              <Tag :color="resolveStatusColor(charterStatus)">
                {{ charterStatus || '未建章程' }}
              </Tag>
            </div>
          </template>
          <template #extra>
            <Space>
              <Button :disabled="!selectedProjectId" @click="openProjectDetail"
                >项目详情</Button
              >
              <Button
                :disabled="!canSaveCharter"
                :loading="savingCharter"
                type="primary"
                @click="handleSaveCharter"
              >
                保存章程
              </Button>
              <Button
                :disabled="
                  !canSaveCharter ||
                  charterStatus === '待审批' ||
                  charterStatus === '已批准'
                "
                :loading="processingApproval === 'submit'"
                type="primary"
                @click="handleSubmitApproval"
              >
                提交审批
              </Button>
              <Button
                :disabled="charterStatus !== '待审批'"
                :loading="processingApproval === 'approve'"
                type="primary"
                @click="handleApproveApproval"
              >
                通过审批
              </Button>
              <Button
                danger
                :disabled="charterStatus !== '待审批'"
                :loading="processingApproval === 'reject'"
                @click="handleRejectApproval"
              >
                驳回
              </Button>
              <Button
                :disabled="
                  charterStatus !== '已批准' ||
                  selectedProject?.status !== '待规划'
                "
                :loading="promotingProject"
                type="primary"
                @click="handlePromoteProject"
              >
                进入执行
              </Button>
            </Space>
          </template>

          <div v-if="!selectedProject" class="project-initiation__empty">
            <Empty description="请选择待立项项目" />
          </div>
          <div v-else class="project-initiation__editor-body">
            <div class="project-initiation__project-head">
              <div>
                <span>{{ selectedProject.projectCode }}</span>
                <h3>{{ selectedProject.name }}</h3>
              </div>
              <div class="project-initiation__project-meta">
                <span>{{ selectedProject.organizationUnitName }}</span>
                <span>{{ selectedProject.managerName }}</span>
                <span
                  >{{ formatDate(selectedProject.plannedStartDate) }} -
                  {{ formatDate(selectedProject.plannedEndDate) }}</span
                >
              </div>
            </div>

            <div class="project-initiation__checks">
              <Tag
                v-for="item in completionItems"
                :key="item.label"
                :color="item.ready ? 'success' : 'default'"
              >
                {{ item.label }}
              </Tag>
            </div>

            <Form class="project-initiation__form" layout="vertical">
              <div class="project-initiation__approval">
                <div>
                  <span>审批状态</span>
                  <Tag :color="resolveStatusColor(charterStatus)">
                    {{ charterStatus || '草稿' }}
                  </Tag>
                </div>
                <div v-if="approvalRecords.length > 0" class="project-initiation__approval-records">
                  <span
                    v-for="record in approvalRecords"
                    :key="record.id"
                  >
                    {{ record.action }} · {{ record.actorUserName || '-' }} · {{ record.fromStatus }} -> {{ record.toStatus }}
                  </span>
                </div>
              </div>
              <div class="project-initiation__form-grid">
                <Form.Item label="商业论证">
                  <Input.TextArea
                    v-model:value="charterForm.businessCase"
                    :auto-size="{ minRows: 3, maxRows: 6 }"
                    :maxlength="2048"
                    placeholder="说明项目必要性、业务价值和投入依据"
                  />
                </Form.Item>
                <Form.Item label="项目目标">
                  <Input.TextArea
                    v-model:value="charterForm.objectives"
                    :auto-size="{ minRows: 3, maxRows: 6 }"
                    :maxlength="2048"
                    placeholder="明确可衡量的交付目标和阶段性结果"
                  />
                </Form.Item>
                <Form.Item label="范围边界">
                  <Input.TextArea
                    v-model:value="charterForm.scopeBoundary"
                    :auto-size="{ minRows: 3, maxRows: 6 }"
                    :maxlength="2048"
                    placeholder="列出包含范围、不包含范围和关键边界"
                  />
                </Form.Item>
                <Form.Item label="成功标准">
                  <Input.TextArea
                    v-model:value="charterForm.successCriteria"
                    :auto-size="{ minRows: 3, maxRows: 6 }"
                    :maxlength="2048"
                    placeholder="定义验收、经营、质量或上线成功标准"
                  />
                </Form.Item>
                <Form.Item label="假设条件">
                  <Input.TextArea
                    v-model:value="charterForm.assumptions"
                    :auto-size="{ minRows: 2, maxRows: 5 }"
                    :maxlength="2048"
                    placeholder="记录启动阶段的资源、时间、外部依赖假设"
                  />
                </Form.Item>
                <Form.Item label="制约因素">
                  <Input.TextArea
                    v-model:value="charterForm.constraints"
                    :auto-size="{ minRows: 2, maxRows: 5 }"
                    :maxlength="2048"
                    placeholder="记录预算、合规、排期、组织边界等约束"
                  />
                </Form.Item>
              </div>
            </Form>
          </div>
        </Card>
      </div>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-initiation {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-initiation__summary {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  @apply gap-6;
  @apply px-0.5 pb-2 pt-1;
}

.project-initiation__eyebrow {
  color: hsl(var(--muted-foreground));
  font-size: 12px;
}

.project-initiation__summary h2 {
  @apply mt-1;
  font-size: 20px;
  font-weight: 600;
}

.project-initiation__metrics {
  display: grid;
  min-width: 420px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  @apply gap-3;
}

.project-initiation__metrics article {
  border-left: 2px solid hsl(var(--primary));
  @apply pl-3;
}

.project-initiation__metrics span,
.project-initiation__project-head span,
.project-initiation__project-meta {
  color: hsl(var(--muted-foreground));
  font-size: 12px;
}

.project-initiation__metrics strong {
  display: block;
  @apply mt-0.5;
  font-size: 20px;
  font-weight: 600;
}

.project-initiation__layout {
  display: grid;
  grid-template-columns: minmax(360px, 0.9fr) minmax(0, 1.4fr);
  @apply gap-4;
  align-items: start;
}

.project-initiation__list,
.project-initiation__editor {
  min-width: 0;
}

.project-initiation__selected-title {
  display: flex;
  align-items: center;
  @apply gap-2;
}

.project-initiation__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

.project-initiation__editor-body {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-initiation__project-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  @apply gap-4;
  border-bottom: 1px solid hsl(var(--border));
  @apply pb-3;
}

.project-initiation__project-head h3 {
  @apply mt-1;
  font-size: 18px;
  font-weight: 600;
}

.project-initiation__project-meta {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  @apply gap-2;
  text-align: right;
}

.project-initiation__checks {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
}

.project-initiation__approval {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.project-initiation__approval > div:first-child {
  display: flex;
  align-items: center;
  @apply gap-2;
}

.project-initiation__approval-records {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.project-initiation__form {
  max-width: 100%;
}

.project-initiation__form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  @apply gap-x-4;
}

@media (max-width: 1180px) {
  .project-initiation__summary,
  .project-initiation__project-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .project-initiation__metrics {
    min-width: 0;
    width: 100%;
  }

  .project-initiation__layout,
  .project-initiation__form-grid {
    grid-template-columns: 1fr;
  }

  .project-initiation__project-meta {
    justify-content: flex-start;
    text-align: left;
  }
}
</style>
