<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { OrganizationApi, UserApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import { useAccess } from '@vben/access';
import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Checkbox,
  Col,
  DatePicker,
  Drawer,
  Empty,
  Form,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Statistic,
  Tag,
  TreeSelect,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  createProjectApi,
  getOrganizationTreeApi,
  getProjectListApi,
  getUserListApi,
  type ProjectApi,
} from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectListPage' });

const PROJECT_CREATE_CODE = 'Project.Project.Create';

type ProjectSaveFormState = Omit<
  ProjectApi.ProjectSaveInput,
  | 'budgetTotalAmount'
  | 'contractTotalAmount'
  | 'description'
  | 'plannedEndDate'
  | 'plannedStartDate'
  | 'projectSource'
  | 'projectType'
  | 'receivedAmount'
  | 'shortName'
> & {
  budgetTotalAmount?: number;
  contractTotalAmount?: number;
  description?: string;
  plannedEndDate?: string;
  plannedStartDate?: string;
  projectSource?: string;
  projectType?: string;
  receivedAmount?: number;
  shortName?: string;
};

const router = useRouter();
const { hasAccessByCodes } = useAccess();
const canCreateProject = computed(() =>
  hasAccessByCodes([PROJECT_CREATE_CODE]),
);
const loading = ref(false);
const saving = ref(false);
const createVisible = ref(false);
const items = ref<ProjectApi.ProjectListItem[]>([]);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const organizationTree = ref<OrganizationApi.OrganizationTreeItem[]>([]);
const userOptions = ref<Array<{ label: string; value: string }>>([]);
const filters = reactive<ProjectApi.ProjectListInput>({
  keyword: '',
  onlyMyRelated: false,
  status: undefined,
});
const formState = reactive<ProjectSaveFormState>({
  budgetTotalAmount: undefined,
  contractTotalAmount: undefined,
  description: undefined,
  isKeyProject: false,
  managerUserId: '',
  name: '',
  organizationUnitId: '',
  plannedEndDate: undefined,
  plannedStartDate: undefined,
  priority: 'P1',
  projectCode: '',
  projectSource: undefined,
  projectType: undefined,
  receivedAmount: undefined,
  shortName: undefined,
  sponsorUserId: '',
  status: '待规划',
});
const projectDicts = useDictOptions(
  ['project_status', 'task_priority', 'project_type', 'project_source'],
  createDictOptionsFallback({
    project_source: ['公司立项', '客户需求', '部门提报', '经营规划'],
    project_status: ['待规划', '进行中', '已完成', '已关闭'],
    project_type: ['平台建设', '业务项目', '客户交付', '内部优化'],
    task_priority: ['P0', 'P1', 'P2', 'P3'],
  }),
);

const columns = [
  { field: 'projectCode', title: '项目编号', width: 140 },
  { align: 'left', field: 'name', slots: { default: 'name' }, title: '项目名称', width: 220 },
  { field: 'organizationUnitName', title: '主责部门', width: 140 },
  { field: 'managerName', title: '项目经理', width: 120 },
  { field: 'memberCount', title: '成员数', width: 96 },
  { field: 'myRelation', slots: { default: 'myRelation' }, title: '我的关系', width: 160 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
  { field: 'priority', slots: { default: 'priority' }, title: '优先级', width: 96 },
  { field: 'highRiskCount', title: '高风险', width: 90 },
  { field: 'plannedEndDate', slots: { default: 'plannedEndDate' }, title: '计划结束', width: 140 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<ProjectApi.ProjectListItem>['columns'];

const metrics = computed(() => [
  { label: '项目总数', value: items.value.length },
  {
    label: '重点项目',
    value: items.value.filter((item) => item.isKeyProject).length,
  },
  {
    label: '与我相关',
    value: items.value.filter((item) => item.myRelation !== '组织可见').length,
  },
  {
    label: '异常项目',
    value: items.value.filter(
      (item) => item.highRiskCount > 0 || item.overdueTaskCount > 0,
    ).length,
  },
]);

const statusOptions = projectDicts.getOptions('project_status');
const priorityOptions = projectDicts.getOptions('task_priority');
const projectTypeOptions = projectDicts.getOptions('project_type');
const projectSourceOptions = projectDicts.getOptions('project_source');

const gridOptions = computed<VxeGridProps<ProjectApi.ProjectListItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const gridEvents: NativeVxeGridEvents<ProjectApi.ProjectListItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<ProjectApi.ProjectListItem>({
  gridEvents,
  gridOptions: {},
});

function normalizeTreeSelect(
  items: OrganizationApi.OrganizationTreeItem[],
): Array<{ children?: any[]; title: string; value: string }> {
  return items.map((item) => ({
    children: normalizeTreeSelect(item.children || []),
    title: item.displayName,
    value: item.id,
  }));
}

async function loadProjects() {
  loading.value = true;
  try {
    items.value = await getProjectListApi({
      keyword: filters.keyword || undefined,
      onlyMyRelated: filters.onlyMyRelated,
      status: filters.status || undefined,
    });
    pagination.resetPage();
  } finally {
    loading.value = false;
  }
}

async function loadCreateOptions() {
  const [tree, users] = await Promise.all([
    getOrganizationTreeApi(),
    getUserListApi({ maxResultCount: 1000 }),
  ]);
  organizationTree.value = tree;
  userOptions.value = users.items.map((item: UserApi.UserListItem) => ({
    label: `${item.username} · ${item.displayName}`,
    value: item.id,
  }));
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function openDetail(record: ProjectApi.ProjectListItem) {
  localStorage.setItem('general:last-project-id', record.id);
  router.push({
    path: '/project/detail',
  });
}

function resetForm() {
  formState.budgetTotalAmount = undefined;
  formState.contractTotalAmount = undefined;
  formState.description = undefined;
  formState.isKeyProject = false;
  formState.managerUserId = '';
  formState.name = '';
  formState.organizationUnitId = '';
  formState.plannedEndDate = undefined;
  formState.plannedStartDate = undefined;
  formState.priority = 'P1';
  formState.projectCode = '';
  formState.projectSource = undefined;
  formState.projectType = undefined;
  formState.receivedAmount = undefined;
  formState.shortName = undefined;
  formState.sponsorUserId = '';
  formState.status = '待规划';
}

async function openCreate() {
  if (!canCreateProject.value) {
    return;
  }

  resetForm();
  createVisible.value = true;
  if (organizationTree.value.length === 0 || userOptions.value.length === 0) {
    await loadCreateOptions();
  }
}

async function handleCreate() {
  if (!formState.projectCode.trim() || !formState.name.trim()) {
    message.warning('请输入项目编号和项目名称');
    return;
  }
  if (
    !formState.organizationUnitId ||
    !formState.managerUserId ||
    !formState.sponsorUserId
  ) {
    message.warning('请选择主责部门、项目经理和发起人');
    return;
  }

  saving.value = true;
  try {
    const projectId = await createProjectApi({
      ...formState,
      description: formState.description || null,
      name: formState.name.trim(),
      projectCode: formState.projectCode.trim(),
      projectSource: formState.projectSource || null,
      projectType: formState.projectType || null,
      shortName: formState.shortName || null,
    });
    message.success('项目已创建');
    createVisible.value = false;
    await loadProjects();
    localStorage.setItem('general:last-project-id', projectId);
    router.push({ path: '/project/detail' });
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await Promise.all([projectDicts.load(), loadProjects()]);
});
</script>

<template>
  <Page
    description="项目列表按组织范围叠加项目成员覆盖返回，并支持直接进入项目详情横向页。"
    title="项目列表"
  >
    <section class="project-list">
      <Row :gutter="[16, 16]">
        <Col
          v-for="metric in metrics"
          :key="metric.label"
          :lg="6"
          :md="12"
          :span="24"
        >
          <Card :bordered="false" class="project-list__metric rounded-2xl">
            <Statistic :title="metric.label" :value="metric.value" />
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" class="project-list__table-card rounded-2xl" title="项目清单">
        <template #extra>
          <div class="project-list__toolbar">
            <Input
              v-model:value="filters.keyword"
              allow-clear
              placeholder="按项目编号、名称、项目经理筛选"
              @pressEnter="loadProjects"
            />
            <Select
              v-model:value="filters.status"
              allow-clear
              class="project-list__select"
              :options="statusOptions"
              placeholder="状态"
            />
            <Button
              :type="filters.onlyMyRelated ? 'primary' : 'default'"
              @click="filters.onlyMyRelated = !filters.onlyMyRelated"
            >
              与我相关
            </Button>
            <Button type="primary" @click="loadProjects">查询</Button>
            <Button v-if="canCreateProject" type="primary" @click="openCreate"
              >新建项目</Button
            >
          </div>
        </template>

        <div v-if="!loading && items.length === 0" class="project-list__empty">
          <Empty description="暂无项目数据" />
        </div>

        <Grid
          v-else
          class="app-vxe-card-grid"
          grid-class="project-list__grid"
          :grid-events="gridEvents"
          :grid-options="gridOptions"
        >
          <template #name="{ row }">
            <button
              class="project-list__link"
              type="button"
              @click="openDetail(row)"
            >
              {{ row.name }}
            </button>
          </template>
          <template #myRelation="{ row }">
            <Tag color="processing">{{ row.myRelation }}</Tag>
          </template>
          <template #status="{ row }">
            <Tag
              :color="
                projectDicts.getColor(
                  'project_status',
                  row.status,
                  row.isKeyProject ? 'gold' : 'blue',
                )
              "
            >
              {{ row.status }}
            </Tag>
          </template>
          <template #priority="{ row }">
            <Tag
              :color="
                projectDicts.getColor(
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
          <template #actions="{ row }">
            <Space>
              <Button size="small" type="link" @click="openDetail(row)">
                查看详情
              </Button>
            </Space>
          </template>
        </Grid>
      </Card>

      <Drawer
        v-model:open="createVisible"
        destroy-on-close
        placement="right"
        title="新建项目"
        width="720"
      >
        <Form class="project-form" layout="vertical">
          <Row :gutter="16">
            <Col :md="12" :span="24">
              <Form.Item label="项目编号" required>
                <Input
                  v-model:value="formState.projectCode"
                  :maxlength="64"
                  placeholder="例如：PRJ-2026-001"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="项目名称" required>
                <Input
                  v-model:value="formState.name"
                  :maxlength="128"
                  placeholder="输入项目名称"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="项目简称">
                <Input
                  v-model:value="formState.shortName"
                  :maxlength="64"
                  placeholder="可选"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="主责部门" required>
                <TreeSelect
                  v-model:value="formState.organizationUnitId"
                  placeholder="选择主责部门"
                  :tree-data="normalizeTreeSelect(organizationTree)"
                  tree-default-expand-all
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="项目经理" required>
                <Select
                  v-model:value="formState.managerUserId"
                  show-search
                  option-filter-prop="label"
                  placeholder="选择项目经理"
                  :options="userOptions"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="发起人" required>
                <Select
                  v-model:value="formState.sponsorUserId"
                  show-search
                  option-filter-prop="label"
                  placeholder="选择发起人"
                  :options="userOptions"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="状态">
                <Select
                  v-model:value="formState.status"
                  :options="statusOptions"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="优先级">
                <Select
                  v-model:value="formState.priority"
                  :options="priorityOptions"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="重点项目">
                <Checkbox v-model:checked="formState.isKeyProject"
                  >标记为重点</Checkbox
                >
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="项目类型">
                <Select
                  v-model:value="formState.projectType"
                  allow-clear
                  :options="projectTypeOptions"
                  placeholder="选择项目类型"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="项目来源">
                <Select
                  v-model:value="formState.projectSource"
                  allow-clear
                  :options="projectSourceOptions"
                  placeholder="选择项目来源"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="计划开始">
                <DatePicker
                  v-model:value="formState.plannedStartDate"
                  class="project-form__full"
                  value-format="YYYY-MM-DD"
                />
              </Form.Item>
            </Col>
            <Col :md="12" :span="24">
              <Form.Item label="计划结束">
                <DatePicker
                  v-model:value="formState.plannedEndDate"
                  class="project-form__full"
                  value-format="YYYY-MM-DD"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="预算金额">
                <InputNumber
                  v-model:value="formState.budgetTotalAmount"
                  class="project-form__full"
                  :min="0"
                  :precision="2"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="合同金额">
                <InputNumber
                  v-model:value="formState.contractTotalAmount"
                  class="project-form__full"
                  :min="0"
                  :precision="2"
                />
              </Form.Item>
            </Col>
            <Col :md="8" :span="24">
              <Form.Item label="已回款">
                <InputNumber
                  v-model:value="formState.receivedAmount"
                  class="project-form__full"
                  :min="0"
                  :precision="2"
                />
              </Form.Item>
            </Col>
            <Col :span="24">
              <Form.Item label="项目说明">
                <Input.TextArea
                  v-model:value="formState.description"
                  :maxlength="512"
                  :rows="4"
                />
              </Form.Item>
            </Col>
          </Row>
        </Form>
        <div class="project-form__footer">
          <Button @click="createVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleCreate"
            >保存并进入详情</Button
          >
        </div>
      </Drawer>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-list {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-list__metric {
  min-height: 100%;
}

.project-list__toolbar {
  display: flex;
  @apply gap-3;
}

.project-list__select {
  width: 160px;
}

.project-list__empty {
  display: grid;
  min-height: 280px;
  place-items: center;
}

.project-list__link {
  @apply p-0;
  color: var(--ant-color-primary);
  background: transparent;
  border: 0;
  cursor: pointer;
  font-weight: 600;
}

.project-form {
  @apply pb-18;
}

.project-form__full {
  width: 100%;
}

.project-form__footer {
  position: sticky;
  bottom: 0;
  display: flex;
  justify-content: flex-end;
  @apply gap-3;
  @apply -mx-6;
  @apply px-6 py-4;
  border-top: 1px solid var(--ant-color-border-secondary);
  background: var(--ant-color-bg-container);
}

@media (max-width: 960px) {
  .project-list__toolbar {
    display: grid;
    grid-template-columns: 1fr;
  }

  .project-list__select {
    width: 100%;
  }
}
</style>
