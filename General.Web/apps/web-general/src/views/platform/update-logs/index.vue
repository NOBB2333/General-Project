<script lang="ts" setup>
import type { UpdateLogApi } from '#/api/core';

import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Form, Input, Modal, Popconfirm, Space, Table, message } from 'ant-design-vue';

import { createUpdateLogApi, deleteUpdateLogApi, getUpdateLogListApi, updateUpdateLogApi } from '#/api/core';

defineOptions({ name: 'PlatformUpdateLogsPage' });

const columns = [
  { dataIndex: 'version', key: 'version', title: '版本', width: 120 },
  { dataIndex: 'title', key: 'title', title: '标题', width: 220 },
  { dataIndex: 'publishedAt', key: 'publishedAt', title: '发布日期', width: 180 },
  { dataIndex: 'authorName', key: 'authorName', title: '发布人', width: 120 },
  { dataIndex: 'impactScope', key: 'impactScope', title: '影响范围' },
  { key: 'actions', title: '操作', width: 160 },
];

const items = ref<UpdateLogApi.UpdateLogItem[]>([]);
const loading = ref(false);
const saving = ref(false);
const modalVisible = ref(false);
const editingId = ref<null | string>(null);
const formState = reactive<UpdateLogApi.UpdateLogSaveInput>({
  impactScope: undefined,
  publishedAt: new Date().toISOString(),
  summary: '',
  title: '',
  version: '',
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
  modalVisible.value = true;
}

function openEdit(item: UpdateLogApi.UpdateLogItem) {
  editingId.value = item.id;
  formState.impactScope = item.impactScope || '';
  formState.publishedAt = item.publishedAt;
  formState.summary = item.summary;
  formState.title = item.title;
  formState.version = item.version;
  modalVisible.value = true;
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

    modalVisible.value = false;
    await loadLogs();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await deleteUpdateLogApi(id);
  message.success('更新日志已删除');
  await loadLogs();
}

onMounted(loadLogs);
</script>

<template>
  <Page description="更新日志用于记录平台版本、上线日期、摘要和影响范围。" title="更新日志">
    <Card :bordered="false" title="版本记录">
      <template #extra>
        <Button type="primary" @click="openCreate">新增日志</Button>
      </template>

      <div v-if="!loading && items.length === 0" class="platform-empty">
        <Empty description="暂无更新日志" />
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
          <template v-if="column.key === 'publishedAt'">
            {{ new Date(record.publishedAt).toLocaleDateString() }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Button size="small" type="link" @click="openEdit(record as UpdateLogApi.UpdateLogItem)">
                编辑
              </Button>
              <Popconfirm title="确认删除该更新日志？" @confirm="handleDelete(record.id)">
                <Button danger size="small" type="link">删除</Button>
              </Popconfirm>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="editingId ? '编辑更新日志' : '新增更新日志'"
      @ok="handleSubmit"
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
    </Modal>
  </Page>
</template>

<style scoped>
.platform-empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
