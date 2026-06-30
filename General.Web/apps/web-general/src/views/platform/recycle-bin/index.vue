<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { RecycleBinApi } from '#/api/core';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Popconfirm, Select, Space, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  deleteRecycleBinItemPermanentlyApi,
  getRecycleBinItemsApi,
  restoreRecycleBinItemApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useDateFormatter } from '#/composables/platform/use-date-formatter';

defineOptions({ name: 'PlatformRecycleBinPage' });

const ENTITY_OPTIONS = [
  { label: '全部', value: '' },
  { label: '菜单', value: 'menu' },
  { label: '字典类型', value: 'dict-type' },
  { label: '字典项', value: 'dict-data' },
  { label: '文件', value: 'file' },
  { label: '定时任务', value: 'scheduled-job' },
  { label: '任务触发器', value: 'scheduled-trigger' },
  { label: '更新日志', value: 'update-log' },
  { label: '开放应用', value: 'open-api' },
];

const columns = [
  { field: 'entityType', slots: { default: 'entityType' }, title: '类型', width: 150 },
  { align: 'left', field: 'displayName', title: '名称', width: 220 },
  { align: 'left', field: 'originalLocation', slots: { default: 'originalLocation' }, title: '原始位置' },
  { field: 'deleterName', slots: { default: 'deleterName' }, title: '删除人', width: 140 },
  { field: 'deletionTime', slots: { default: 'deletionTime' }, title: '删除时间', width: 190 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 180 },
] satisfies VxeGridProps<RecycleBinApi.RecycleBinItem>['columns'];

const entityType = ref('');
const items = ref<RecycleBinApi.RecycleBinItem[]>([]);
const loading = ref(false);
const pageIndex = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const { actionLoadingKey, runAction } = useActionLoading();
const { formatDateTime } = useDateFormatter();

const gridOptions = computed<VxeGridProps<RecycleBinApi.RecycleBinItem>>(() => ({
  columns,
  data: items.value,
  loading: loading.value,
  pagerConfig: {
    currentPage: pageIndex.value,
    pageSize: pageSize.value,
    total: totalCount.value,
  },
  rowConfig: { keyField: 'id' },
}));

const gridEvents: NativeVxeGridEvents<RecycleBinApi.RecycleBinItem> = {
  'page-change': ({ currentPage, pageSize: nextPageSize }) => {
    const normalizedPageSize = nextPageSize || pageSize.value;
    pageIndex.value = normalizedPageSize === pageSize.value ? currentPage || 1 : 1;
    pageSize.value = normalizedPageSize;
    void loadItems();
  },
};

const [Grid] = useVbenVxeGrid<RecycleBinApi.RecycleBinItem>({
  gridEvents,
  gridOptions: {},
});

async function loadItems() {
  loading.value = true;
  try {
    const result = await getRecycleBinItemsApi({
      entityType: entityType.value || undefined,
      maxResultCount: pageSize.value,
      skipCount: (pageIndex.value - 1) * pageSize.value,
    });
    items.value = result.items;
    totalCount.value = result.totalCount;
  } finally {
    loading.value = false;
  }
}

function handleEntityTypeChange() {
  pageIndex.value = 1;
  void loadItems();
}

async function handleRestore(record: RecycleBinApi.RecycleBinItem) {
  await runAction(`restore:${record.entityType}:${record.id}`, async () => {
    await restoreRecycleBinItemApi(record.entityType, record.id);
    message.success('数据已恢复');
    await loadItems();
  });
}

async function handleDeletePermanently(record: RecycleBinApi.RecycleBinItem) {
  await runAction(`delete:${record.entityType}:${record.id}`, async () => {
    await deleteRecycleBinItemPermanentlyApi(record.entityType, record.id);
    message.success('数据已彻底删除');
    await loadItems();
  });
}

onMounted(loadItems);
</script>

<template>
  <Page description="回收站用于恢复核心主数据的软删除记录，日志和执行流水仍走清理策略。" title="回收站">
    <Card class="rounded-2xl" :bordered="false" title="已删除数据">
      <template #extra>
        <Space>
          <Select
            v-model:value="entityType"
            :options="ENTITY_OPTIONS"
            style="width: 160px"
            @change="handleEntityTypeChange"
          />
          <Button @click="loadItems">刷新</Button>
        </Space>
      </template>

      <Grid :grid-events="gridEvents" :grid-options="gridOptions">
        <template #entityType="{ row }">
          <Tag>{{ row.entityType }}</Tag>
        </template>
        <template #deletionTime="{ row }">
          {{ formatDateTime(row.deletionTime) }}
        </template>
        <template #deleterName="{ row }">
          {{ row.deleterName || '系统' }}
        </template>
        <template #originalLocation="{ row }">
          {{ row.originalLocation || '-' }}
        </template>
        <template #actions="{ row }">
          <Space>
            <Popconfirm title="确认恢复该数据？" @confirm="handleRestore(row)">
              <Button
                :loading="actionLoadingKey === `restore:${row.entityType}:${row.id}`"
                size="small"
                type="link"
              >
                恢复
              </Button>
            </Popconfirm>
            <Popconfirm
              v-if="row.entityType === 'file'"
              title="确认彻底删除该文件？物理文件也会被删除。"
              @confirm="handleDeletePermanently(row)"
            >
              <Button
                danger
                :loading="actionLoadingKey === `delete:${row.entityType}:${row.id}`"
                size="small"
                type="link"
              >
                彻底删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </Grid>
    </Card>
  </Page>
</template>
