<script lang="ts" setup>
import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Card, Empty, Table, Tag } from 'ant-design-vue';

import { getOnlineUserListApi, type OnlineUserApi } from '#/api/core';

defineOptions({ name: 'PlatformOnlineUsersPage' });

const items = ref<OnlineUserApi.OnlineUserItem[]>([]);
const loading = ref(false);

const columns = [
  { dataIndex: 'userName', key: 'userName', title: '账号' },
  { dataIndex: 'tenantName', key: 'tenantName', title: '租户', width: 120 },
  { dataIndex: 'device', key: 'device', title: '设备', width: 120 },
  { dataIndex: 'browser', key: 'browser', title: '浏览器' },
  { dataIndex: 'ipAddress', key: 'ipAddress', title: 'IP', width: 160 },
  { dataIndex: 'lastAccessedAt', key: 'lastAccessedAt', title: '最近活动', width: 200 },
];

async function loadOnlineUsers() {
  loading.value = true;
  try {
    items.value = await getOnlineUserListApi();
  } finally {
    loading.value = false;
  }
}

onMounted(loadOnlineUsers);
</script>

<template>
  <Page description="在线用户页基于当前会话表展示最近在线账号与活动时间。" title="在线用户">
    <Card :bordered="false" title="在线会话">
      <div v-if="!loading && items.length === 0" class="platform-empty">
        <Empty description="暂无在线用户" />
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
          <template v-if="column.key === 'tenantName'">
            <Tag color="blue">{{ record.tenantName || '默认租户' }}</Tag>
          </template>
          <template v-else-if="column.key === 'lastAccessedAt'">
            {{ record.lastAccessedAt ? new Date(record.lastAccessedAt).toLocaleString() : '-' }}
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>

<style scoped>
.platform-empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
