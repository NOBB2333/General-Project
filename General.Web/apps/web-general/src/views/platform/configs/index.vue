<script lang="ts" setup>
import type { ConfigApi } from '#/api/core';

import { onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Form, Input, InputNumber, Modal, Space, Table, Tag, message } from 'ant-design-vue';

import { getPlatformConfigListApi, updatePlatformConfigApi } from '#/api/core';

defineOptions({ name: 'PlatformConfigsPage' });

const columns = [
  { dataIndex: 'groupCode', key: 'groupCode', title: '分组', width: 110 },
  { dataIndex: 'name', key: 'name', title: '名称', width: 160 },
  { dataIndex: 'code', key: 'code', title: '配置编码', width: 280 },
  { dataIndex: 'value', key: 'value', title: '当前值', width: 180 },
  { dataIndex: 'valueType', key: 'valueType', title: '类型', width: 90 },
  { dataIndex: 'description', key: 'description', title: '说明' },
  { key: 'actions', title: '操作', width: 100 },
];

const items = ref<ConfigApi.ConfigItem[]>([]);
const loading = ref(false);
const saving = ref(false);
const modalVisible = ref(false);
const editingItem = ref<ConfigApi.ConfigItem | null>(null);
const formState = reactive({
  numberValue: 0,
  value: '',
});

async function loadConfigs() {
  loading.value = true;
  try {
    items.value = await getPlatformConfigListApi();
  } finally {
    loading.value = false;
  }
}

function openEdit(item: ConfigApi.ConfigItem) {
  editingItem.value = item;
  formState.value = item.value;
  formState.numberValue = Number.parseInt(item.value, 10) || 0;
  modalVisible.value = true;
}

async function handleSubmit() {
  if (!editingItem.value) {
    return;
  }

  const nextValue = editingItem.value.valueType === 'number' ? `${formState.numberValue}` : formState.value.trim();
  if (!nextValue) {
    message.warning('请输入配置值');
    return;
  }

  saving.value = true;
  try {
    await updatePlatformConfigApi(editingItem.value.code, nextValue);
    message.success('配置已保存');
    modalVisible.value = false;
    await loadConfigs();
  } finally {
    saving.value = false;
  }
}

onMounted(loadConfigs);
</script>

<template>
  <Page description="配置参数用于维护允许在线调整的安全运行参数，基础设施连接与密钥仍应通过配置文件或环境变量管理。" title="配置参数">
    <Card :bordered="false" title="参数列表">
      <Table
        :columns="columns"
        :data-source="items"
        :loading="loading"
        :pagination="{ pageSize: 10 }"
        row-key="code"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'groupCode'">
            <Tag>{{ record.groupCode }}</Tag>
          </template>
          <template v-else-if="column.key === 'valueType'">
            <Tag color="blue">{{ record.valueType }}</Tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <Space>
              <Button :disabled="record.isReadonly" size="small" type="link" @click="openEdit(record as ConfigApi.ConfigItem)">
                编辑
              </Button>
            </Space>
          </template>
        </template>
      </Table>
    </Card>

    <Modal
      v-model:open="modalVisible"
      :confirm-loading="saving"
      :title="editingItem ? `编辑配置 · ${editingItem.name}` : '编辑配置'"
      @ok="handleSubmit"
    >
      <Form layout="vertical">
        <Form.Item label="配置编码">
          <Input :value="editingItem?.code || ''" disabled />
        </Form.Item>
        <Form.Item label="配置值" required>
          <InputNumber
            v-if="editingItem?.valueType === 'number'"
            v-model:value="formState.numberValue"
            :min="0"
            style="width: 100%"
          />
          <Input v-else v-model:value="formState.value" :maxlength="256" />
        </Form.Item>
        <Form.Item label="说明">
          <Input.TextArea :value="editingItem?.description || ''" disabled :rows="3" />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>
