<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Input, Select, Switch, Table, Tag } from 'ant-design-vue';

import { getProjectRaidListApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectRaidPage' });

const loading = ref(false);
const items = ref<ProjectApi.RaidItem[]>([]);
const filters = reactive<ProjectApi.RaidListInput>({
  keyword: '',
  onlyOpen: true,
  type: undefined,
});

const columns = [
  { dataIndex: 'type', key: 'type', title: '类型', width: 110 },
  { dataIndex: 'title', key: 'title', title: '事项标题', width: 240 },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 180 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '责任人', width: 120 },
  { dataIndex: 'level', key: 'level', title: '等级', width: 100 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 120 },
  { dataIndex: 'plannedResolveDate', key: 'plannedResolveDate', title: '计划处理', width: 160 },
];

const typeOptions = ['风险', '问题', '依赖', '假设'].map((value) => ({
  label: value,
  value,
}));

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

onMounted(loadRaidItems);
</script>

<template>
  <Page description="RAID 视图统一汇总风险、问题、依赖、假设，并优先暴露高风险和超期事项。" title="RAID 视图">
    <Card :bordered="false" title="治理事项">
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
      <Table
        v-else
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="{ pageSize: 10 }"
        row-key="id"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'type'">
            <Tag :color="(record as ProjectApi.RaidItem).type === '风险' ? 'error' : 'blue'">
              {{ (record as ProjectApi.RaidItem).type }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'level'">
            <Tag :color="(record as ProjectApi.RaidItem).level === '高' ? 'error' : 'warning'">
              {{ (record as ProjectApi.RaidItem).level }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'status'">
            <Tag :color="(record as ProjectApi.RaidItem).isOverdue ? 'error' : 'processing'">
              {{ (record as ProjectApi.RaidItem).status }}
            </Tag>
          </template>
          <template v-else-if="column.key === 'plannedResolveDate'">
            {{ formatDate((record as ProjectApi.RaidItem).plannedResolveDate) }}
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>

<style scoped>
.project-raid__toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
}

.project-raid__select {
  width: 160px;
}

.project-raid__switch {
  display: inline-flex;
  gap: 8px;
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
