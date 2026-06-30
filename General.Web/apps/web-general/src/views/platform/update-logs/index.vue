<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { UpdateLogApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Empty, Form, Input, Popconfirm, Space, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { createUpdateLogApi, deleteUpdateLogApi, getUpdateLogListApi, updateUpdateLogApi } from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformUpdateLogsPage' });

const columns = [
  { field: 'version', title: '版本', width: 120 },
  { align: 'left', field: 'title', title: '标题', width: 220 },
  { field: 'publishedAt', slots: { default: 'publishedAt' }, title: '发布日期', width: 180 },
  { field: 'authorName', title: '发布人', width: 120 },
  { align: 'left', field: 'impactScope', title: '影响范围' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 160 },
] satisfies VxeGridProps<UpdateLogApi.UpdateLogItem>['columns'];

const items = ref<UpdateLogApi.UpdateLogItem[]>([]);
const loading = ref(false);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const saving = ref(false);
const drawerVisible = ref(false);
const editingId = ref<null | string>(null);
const { actionLoadingKey, runAction } = useActionLoading();
const formState = reactive<UpdateLogApi.UpdateLogSaveInput>({
  impactScope: undefined,
  publishedAt: new Date().toISOString(),
  summary: '',
  title: '',
  version: '',
});

const gridOptions = computed<VxeGridProps<UpdateLogApi.UpdateLogItem>>(() => ({
  columns,
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
}));

const gridEvents: NativeVxeGridEvents<UpdateLogApi.UpdateLogItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<UpdateLogApi.UpdateLogItem>({
  gridEvents,
  gridOptions: {},
});

async function loadLogs() {
  loading.value = true;
  try {
    items.value = await getUpdateLogListApi();
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  editingId.value = null;
  formState.impactScope = '';
  formState.publishedAt = new Date().toISOString();
  formState.summary = '';
  formState.title = '';
  formState.version = '';
  drawerVisible.value = true;
}

function openEdit(item: UpdateLogApi.UpdateLogItem) {
  editingId.value = item.id;
  formState.impactScope = item.impactScope || '';
  formState.publishedAt = item.publishedAt;
  formState.summary = item.summary;
  formState.title = item.title;
  formState.version = item.version;
  drawerVisible.value = true;
}

async function handleSubmit() {
  if (!formState.version.trim() || !formState.title.trim() || !formState.summary.trim()) {
    message.warning('请填写版本、标题和摘要');
    return;
  }

  saving.value = true;
  try {
    if (editingId.value) {
      await updateUpdateLogApi(editingId.value, formState);
      message.success('更新日志已修改');
    } else {
      await createUpdateLogApi(formState);
      message.success('更新日志已创建');
    }

    drawerVisible.value = false;
    await loadLogs();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await runAction(`delete:${id}`, async () => {
    await deleteUpdateLogApi(id);
    message.success('更新日志已删除');
    await loadLogs();
  });
}

onMounted(loadLogs);
</script>

<template>
  <Page description="更新日志用于记录平台版本、上线日期、摘要和影响范围。" title="更新日志">
    <Card class="rounded-2xl" :bordered="false" title="版本记录">
      <template #extra>
        <Button type="primary" @click="openCreate">新增日志</Button>
      </template>

      <div v-if="!loading && items.length === 0" class="platform-empty">
        <Empty description="暂无更新日志" />
      </div>
      <Grid
        v-else
        :grid-events="gridEvents"
        :grid-options="gridOptions"
      >
        <template #publishedAt="{ row }">
          {{ new Date(row.publishedAt).toLocaleDateString() }}
        </template>
        <template #actions="{ row }">
          <Space>
            <Button size="small" type="link" @click="openEdit(row)">
              编辑
            </Button>
            <Popconfirm title="确认删除该更新日志？" @confirm="handleDelete(row.id)">
              <Button
                danger
                :loading="actionLoadingKey === `delete:${row.id}`"
                size="small"
                type="link"
              >
                删除
              </Button>
            </Popconfirm>
          </Space>
        </template>
      </Grid>
    </Card>

    <Drawer
      v-model:open="drawerVisible"
      :title="editingId ? '编辑更新日志' : '新增更新日志'"
      destroy-on-close
      width="640"
    >
      <Form layout="vertical">
        <Form.Item label="版本号" required>
          <Input v-model:value="formState.version" :maxlength="64" />
        </Form.Item>
        <Form.Item label="标题" required>
          <Input v-model:value="formState.title" :maxlength="128" />
        </Form.Item>
        <Form.Item label="影响范围">
          <Input
            :value="formState.impactScope || ''"
            :maxlength="256"
            @update:value="(value) => { formState.impactScope = value || undefined; }"
          />
        </Form.Item>
        <Form.Item label="发布日期">
          <Input v-model:value="formState.publishedAt" />
        </Form.Item>
        <Form.Item label="变更摘要" required>
          <Input.TextArea v-model:value="formState.summary" :maxlength="2048" :rows="5" />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="drawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </div>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
.platform-empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
