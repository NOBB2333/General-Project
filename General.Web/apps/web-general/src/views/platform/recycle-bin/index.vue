<script lang="ts" setup>
import type { RecycleBinApi } from '#/api/core';

import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Popconfirm, Select, Space, Table, Tag, message } from 'ant-design-vue';

import { getRecycleBinItemsApi, restoreRecycleBinItemApi } from '#/api/core';

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
  { dataIndex: 'displayName', key: 'displayName', title: '名称' },
  { dataIndex: 'deletionTime', key: 'deletionTime', title: '删除时间', width: 190 },
  { key: 'actions', title: '操作', width: 120 },
];

const entityType = ref('');
const items = ref<RecycleBinApi.RecycleBinItem[]>([]);
const loading = ref(false);

async function loadItems() {
  loading.value = true;
  try {
    items.value = await getRecycleBinItemsApi(entityType.value || undefined);
  } finally {
    loading.value = false;
  }
}

async function handleRestore(record: RecycleBinApi.RecycleBinItem) {
  await restoreRecycleBinItemApi(record.entityType, record.id);
  message.success('数据已恢复');
  await loadItems();
}

function formatTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
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
            @change="loadItems"
          />
          <Button @click="loadItems">刷新</Button>
        </Space>
      </template>

      <Table :columns="columns" :data-source="items" :loading="loading" :pagination="{ pageSize: 10 }" row-key="id">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'entityType'">
            <Tag>{{ record.entityType }}</Tag>
          </template>
          <template v-else-if="column.key === 'deletionTime'">
            {{ formatTime(record.deletionTime) }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <Popconfirm title="确认恢复该数据？" @confirm="handleRestore(record as RecycleBinApi.RecycleBinItem)">
              <Button size="small" type="link">恢复</Button>
            </Popconfirm>
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>
