<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Space, Switch, Table, Tag, message } from 'ant-design-vue';

import {
  getSchedulerListApi,
  runSchedulerJobApi,
  toggleSchedulerJobApi,
  type SchedulerApi,
} from '#/api/core';

defineOptions({ name: 'PlatformSchedulerPage' });

const loading = ref(false);
const actionLoadingKey = ref('');
const items = ref<SchedulerApi.JobItem[]>([]);

const columns = [
  { dataIndex: 'title', key: 'title', title: '任务名称', width: 180 },
  { dataIndex: 'jobKey', key: 'jobKey', title: '任务键', width: 180 },
  { dataIndex: 'cronExpression', key: 'cronExpression', title: 'Cron', width: 180 },
  { dataIndex: 'description', key: 'description', title: '说明', ellipsis: true, width: 320 },
  { dataIndex: 'nextRunTime', key: 'nextRunTime', title: '下次执行', width: 160 },
  { dataIndex: 'lastRunTime', key: 'lastRunTime', title: '最近执行', width: 160 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 100 },
  { key: 'actions', title: '操作', width: 160 },
];

async function loadData() {
  loading.value = true;
  try {
    items.value = await getSchedulerListApi();
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleString() : '-';
}

async function toggleJob(item: SchedulerApi.JobItem, isEnabled: boolean) {
  actionLoadingKey.value = `${item.jobKey}:toggle`;
  try {
    await toggleSchedulerJobApi(item.jobKey, isEnabled);
    message.success(isEnabled ? '已启用' : '已停用');
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

async function runJob(item: SchedulerApi.JobItem) {
  actionLoadingKey.value = `${item.jobKey}:run`;
  try {
    await runSchedulerJobApi(item.jobKey);
    message.success('已触发手动执行');
    await loadData();
  } finally {
    actionLoadingKey.value = '';
  }
}

onMounted(loadData);
</script>

<template>
  <Page description="一期先落一个轻量定时任务中心，用于查看内置任务、启停和手动执行。" title="定时任务">
    <Card :bordered="false" title="任务中心">
      <div v-if="!loading && items.length === 0" class="scheduler__empty">
        <Empty description="暂无定时任务" />
      </div>
      <Table
        v-else
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="false"
        row-key="jobKey"
        :scroll="{ x: 1320 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'description'">
            <div class="scheduler__desc">{{ (record as SchedulerApi.JobItem).description || '-' }}</div>
          </template>
          <template v-if="column.key === 'nextRunTime'">
            {{ formatDate((record as SchedulerApi.JobItem).nextRunTime) }}
          </template>
          <template v-else-if="column.key === 'lastRunTime'">
            <div class="scheduler__cell">
              <span>{{ formatDate((record as SchedulerApi.JobItem).lastRunTime) }}</span>
              <small>{{ (record as SchedulerApi.JobItem).lastRunResult || '-' }}</small>
            </div>
          </template>
          <template v-else-if="column.key === 'isEnabled'">
            <Tag :color="(record as SchedulerApi.JobItem).isEnabled ? 'success' : 'default'">
              {{ (record as SchedulerApi.JobItem).isEnabled ? '运行中' : '已停用' }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Switch
                :checked="(record as SchedulerApi.JobItem).isEnabled"
                :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:toggle`"
                @change="(checked) => toggleJob(record as SchedulerApi.JobItem, Boolean(checked))"
              />
              <Button
                size="small"
                :loading="actionLoadingKey === `${(record as SchedulerApi.JobItem).jobKey}:run`"
                @click="runJob(record as SchedulerApi.JobItem)"
              >
                立即执行
              </Button>
            </Space>
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>

<style scoped>
.scheduler__empty {
  display: grid;
  min-height: 260px;
  place-items: center;
}

.scheduler__cell {
  display: flex;
  flex-direction: column;
}

.scheduler__cell small {
  color: var(--ant-color-text-secondary);
}

.scheduler__desc {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  line-height: 1.6;
}
</style>
