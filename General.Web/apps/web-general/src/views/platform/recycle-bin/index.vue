<script lang="ts" setup>
import type { RecycleBinApi } from '#/api/core';

import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Popconfirm, Select, Space, Table, Tag, message } from 'ant-design-vue';

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
  { dataIndex: 'entityType', key: 'entityType', title: '类型', width: 150 },
  { dataIndex: 'displayName', key: 'displayName', title: '名称', width: 220 },
  { dataIndex: 'originalLocation', key: 'originalLocation', title: '原始位置' },
  { dataIndex: 'deleterName', key: 'deleterName', title: '删除人', width: 140 },
  { dataIndex: 'deletionTime', key: 'deletionTime', title: '删除时间', width: 190 },
  { key: 'actions', title: '操作', width: 180 },
];

const entityType = ref('');
const items = ref<RecycleBinApi.RecycleBinItem[]>([]);
const loading = ref(false);
const pageIndex = ref(1);
const pageSize = ref(20);
const totalCount = ref(0);
const { actionLoadingKey, runAction } = useActionLoading();
const { formatDateTime } = useDateFormatter();

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

function handleTableChange(pagination: { current?: number; pageSize?: number }) {
  pageIndex.value = pagination.current || 1;
  pageSize.value = pagination.pageSize || pageSize.value;
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
    <Card :bordered="false" title="已删除数据">
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

      <Table
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="{
          current: pageIndex,
          pageSize,
          showSizeChanger: true,
          showTotal: (total: number) => `共 ${total} 条`,
          total: totalCount,
        }"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'entityType'">
            <Tag>{{ record.entityType }}</Tag>
          </template>
          <template v-else-if="column.key === 'deletionTime'">
            {{ formatDateTime(record.deletionTime) }}
          </template>
          <template v-else-if="column.key === 'deleterName'">
            {{ record.deleterName || '系统' }}
          </template>
          <template v-else-if="column.key === 'originalLocation'">
            {{ record.originalLocation || '-' }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Popconfirm title="确认恢复该数据？" @confirm="handleRestore(record as RecycleBinApi.RecycleBinItem)">
                <Button
                  :loading="actionLoadingKey === `restore:${(record as RecycleBinApi.RecycleBinItem).entityType}:${(record as RecycleBinApi.RecycleBinItem).id}`"
                  size="small"
                  type="link"
                >
                  恢复
                </Button>
              </Popconfirm>
              <Popconfirm
                v-if="(record as RecycleBinApi.RecycleBinItem).entityType === 'file'"
                title="确认彻底删除该文件？物理文件也会被删除。"
                @confirm="handleDeletePermanently(record as RecycleBinApi.RecycleBinItem)"
              >
                <Button
                  danger
                  :loading="actionLoadingKey === `delete:${(record as RecycleBinApi.RecycleBinItem).entityType}:${(record as RecycleBinApi.RecycleBinItem).id}`"
                  size="small"
                  type="link"
                >
                  彻底删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>
