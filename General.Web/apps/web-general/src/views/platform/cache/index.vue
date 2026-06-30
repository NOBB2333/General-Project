<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { CacheApi } from '#/api/core';

import { computed, onMounted, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Popconfirm, Space, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getPlatformCacheAreasApi, refreshPlatformCacheAreaApi } from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformCachePage' });

const columns = [
  { field: 'area', slots: { default: 'area' }, title: '缓存域', width: 160 },
  { align: 'left', field: 'description', title: '说明' },
  { field: 'version', slots: { default: 'version' }, title: '当前版本', width: 180 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<CacheApi.CacheAreaItem>['columns'];

const items = ref<CacheApi.CacheAreaItem[]>([]);
const loading = ref(false);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const { actionLoadingKey, runAction } = useActionLoading();

const gridOptions = computed<VxeGridProps<CacheApi.CacheAreaItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'area' },
}));

const gridEvents: NativeVxeGridEvents<CacheApi.CacheAreaItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<CacheApi.CacheAreaItem>({
  gridEvents,
  gridOptions: {},
});

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
    <Card :bordered="false" class="rounded-2xl" title="平台缓存域">
      <template #extra>
        <Button :loading="loading" @click="loadAreas">刷新</Button>
      </template>

      <Grid
        class="app-vxe-card-grid"
        :grid-events="gridEvents"
        :grid-options="gridOptions"
      >
        <template #area="{ row }">
          <Tag color="blue">{{ row.area }}</Tag>
        </template>
        <template #version="{ row }">
          <code>{{ row.version }}</code>
        </template>
        <template #actions="{ row }">
          <Space>
            <Popconfirm :title="`确认刷新 ${row.area} 缓存域？`" @confirm="refreshArea(row.area)">
              <Button
                :loading="actionLoadingKey === `refresh:${row.area}`"
                size="small"
                type="primary"
              >
                刷新
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </Grid>
    </Card>
  </Page>
</template>
