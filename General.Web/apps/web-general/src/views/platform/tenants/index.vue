<script lang="ts" setup>
import { reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Empty, Form, Input, Modal, Popconfirm, Space, Table, message } from 'ant-design-vue';

import { createTenantApi, deleteTenantApi, getTenantListApi, type TenantApi } from '#/api/core';

defineOptions({ name: 'PlatformTenantPage' });

const columns = [
  { dataIndex: 'name', key: 'name', title: '租户名称' },
  { dataIndex: 'defaultConnectionString', key: 'defaultConnectionString', title: '默认连接串' },
  { key: 'actions', title: '操作', width: 120 },
];

const items = ref<TenantApi.TenantItem[]>([]);
const loading = ref(false);
const modalVisible = ref(false);
const saving = ref(false);
const formState = reactive({
  defaultConnectionString: '',
  name: '',
});

async function loadTenants() {
  loading.value = true;
  try {
    items.value = await getTenantListApi();
  } finally {
    loading.value = false;
  }
}

function openCreate() {
  formState.name = '';
  formState.defaultConnectionString = '';
  modalVisible.value = true;
}

async function handleSubmit() {
  if (!formState.name.trim()) {
    message.warning('请输入租户名称');
    return;
  }

  saving.value = true;
  try {
    await createTenantApi({
      defaultConnectionString: formState.defaultConnectionString || null,
      name: formState.name.trim(),
    });
    message.success('租户已创建');
    modalVisible.value = false;
    await loadTenants();
  } finally {
    saving.value = false;
  }
}

async function handleDelete(id: string) {
  await deleteTenantApi(id);
  message.success('租户已删除');
  await loadTenants();
}

loadTenants();
</script>

<template>
  <Page description="租户管理用于维护多租户空间与后续隔离扩展。" title="租户管理">
    <Card :bordered="false" title="租户列表">
      <template #extra>
        <Button type="primary" @click="openCreate">新增租户</Button>
      </template>

      <div v-if="!loading && items.length === 0" class="platform-empty">
        <Empty description="暂无租户" />
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
          <template v-if="column.key === 'actions'">
            <Space>
              <Popconfirm title="确认删除该租户？" @confirm="handleDelete(record.id)">
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
      title="新增租户"
      @ok="handleSubmit"
    >
      <Form layout="vertical">
        <Form.Item label="租户名称" required>
          <Input v-model:value="formState.name" :maxlength="64" placeholder="例如：demo-tenant" />
        </Form.Item>
        <Form.Item label="默认连接串">
          <Input
            v-model:value="formState.defaultConnectionString"
            :maxlength="512"
            placeholder="可选，不填则沿用默认数据库"
          />
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
