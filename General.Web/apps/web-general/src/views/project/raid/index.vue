<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Input,
  Select,
  Switch,
  Tag,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getProjectRaidListApi, type ProjectApi } from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectRaidPage' });

const loading = ref(false);
const items = ref<ProjectApi.RaidItem[]>([]);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const filters = reactive<ProjectApi.RaidListInput>({
  keyword: '',
  onlyOpen: true,
  type: undefined,
});
const raidDicts = useDictOptions(
  ['raid_type', 'task_priority'],
  createDictOptionsFallback({
    raid_type: ['风险', '问题', '依赖', '假设'],
    task_priority: ['P0', 'P1', 'P2', 'P3'],
  }),
);

const columns = [
  { field: 'type', slots: { default: 'type' }, title: '类型', width: 110 },
  { align: 'left', field: 'title', title: '事项标题', width: 240 },
  { field: 'projectName', title: '所属项目', width: 180 },
  { field: 'ownerName', title: '责任人', width: 120 },
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 100 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'plannedResolveDate', slots: { default: 'plannedResolveDate' }, title: '计划处理', width: 160 },
] satisfies VxeGridProps<ProjectApi.RaidItem>['columns'];

const typeOptions = raidDicts.getOptions('raid_type');

const gridOptions = computed<VxeGridProps<ProjectApi.RaidItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
}));

const gridEvents: NativeVxeGridEvents<ProjectApi.RaidItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<ProjectApi.RaidItem>({
  gridEvents,
  gridOptions: {},
});

async function loadRaidItems() {
  loading.value = true;
  try {
    items.value = await getProjectRaidListApi({
      keyword: filters.keyword || undefined,
      onlyOpen: filters.onlyOpen,
      type: filters.type || undefined,
    });
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

onMounted(async () => {
  await Promise.all([raidDicts.load(), loadRaidItems()]);
});
</script>

<template>
  <Page
    description="RAID 视图统一汇总风险、问题、依赖、假设，并优先暴露高风险和超期事项。"
    title="RAID 视图"
  >
    <Card class="rounded-2xl" :bordered="false" title="治理事项">
      <template #extra>
        <div class="project-raid__toolbar">
          <Input
            v-model:value="filters.keyword"
            allow-clear
            placeholder="按事项标题、项目筛选"
            @pressEnter="loadRaidItems"
          />
          <Select
            v-model:value="filters.type"
            allow-clear
            class="project-raid__select"
            :options="typeOptions"
            placeholder="类型"
          />
          <label class="project-raid__switch">
            <span>仅未关闭</span>
            <Switch v-model:checked="filters.onlyOpen" />
          </label>
          <Button type="primary" @click="loadRaidItems">查询</Button>
        </div>
      </template>

      <div v-if="!loading && items.length === 0" class="project-raid__empty">
        <Empty description="暂无 RAID 数据" />
      </div>
      <Grid
        v-else
        :grid-events="gridEvents"
        :grid-options="gridOptions"
      >
        <template #type="{ row }">
          <Tag :color="raidDicts.getColor('raid_type', row.type, 'blue')">
            {{ row.type }}
          </Tag>
        </template>
        <template #level="{ row }">
          <Tag :color="raidDicts.getColor('task_priority', row.level, 'warning')">
            {{ row.level }}
          </Tag>
        </template>
        <template #status="{ row }">
          <Tag :color="row.isOverdue ? 'error' : 'processing'">
            {{ row.status }}
          </Tag>
        </template>
        <template #plannedResolveDate="{ row }">
          {{ formatDate(row.plannedResolveDate) }}
        </template>
      </Grid>
    </Card>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-raid__toolbar {
  display: flex;
  @apply gap-3;
  align-items: center;
}

.project-raid__select {
  width: 160px;
}

.project-raid__switch {
  display: inline-flex;
  @apply gap-2;
  align-items: center;
  color: var(--ant-color-text-secondary);
}

.project-raid__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

@media (max-width: 960px) {
  .project-raid__toolbar {
    display: grid;
    grid-template-columns: 1fr;
    align-items: stretch;
  }

  .project-raid__select {
    width: 100%;
  }
}
</style>
