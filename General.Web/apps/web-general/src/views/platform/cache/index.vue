<script lang="ts" setup>
import type { CacheApi } from '#/api/core';

import { onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Popconfirm, Space, Table, Tag, message } from 'ant-design-vue';

import { getPlatformCacheAreasApi, refreshPlatformCacheAreaApi } from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

defineOptions({ name: 'PlatformCachePage' });

const columns = [
  { dataIndex: 'area', key: 'area', title: '缓存域', width: 160 },
  { dataIndex: 'description', key: 'description', title: '说明' },
  { dataIndex: 'version', key: 'version', title: '当前版本', width: 180 },
  { key: 'actions', title: '操作', width: 120 },
];

const items = ref<CacheApi.CacheAreaItem[]>([]);
const loading = ref(false);
const { actionLoadingKey, runAction } = useActionLoading();

async function loadAreas() {
  loading.value = true;
  try {
    items.value = await getPlatformCacheAreasApi();
  } finally {
    loading.value = false;
  }
}

async function refreshArea(area: string) {
  await runAction(`refresh:${area}`, async () => {
    await refreshPlatformCacheAreaApi(area);
    message.success('缓存域已刷新');
    await loadAreas();
  });
}

onMounted(loadAreas);
</script>

<template>
  <Page description="缓存管理只维护平台已知缓存域，通过刷新版本号让业务缓存自然失效，不绑定具体 Redis Key 实现。" title="缓存管理">
    <Card :bordered="false" title="平台缓存域">
      <template #extra>
        <Button :loading="loading" @click="loadAreas">刷新</Button>
      </template>

      <Table
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="false"
        row-key="area"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'area'">
            <Tag color="blue">{{ record.area }}</Tag>
          </template>
          <template v-else-if="column.key === 'version'">
            <code>{{ record.version }}</code>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Popconfirm :title="`确认刷新 ${record.area} 缓存域？`" @confirm="refreshArea(record.area)">
                <Button
                  :loading="actionLoadingKey === `refresh:${(record as CacheApi.CacheAreaItem).area}`"
                  size="small"
                  type="primary"
                >
                  刷新
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>
  </Page>
</template>
