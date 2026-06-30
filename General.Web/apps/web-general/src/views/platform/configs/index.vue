<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { ConfigApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import { Button, Card, Drawer, Form, Input, InputNumber, Radio, Space, Tag, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getPlatformConfigListApi, updatePlatformConfigApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformConfigsPage' });

const columns = [
  { field: 'groupName', filters: [], slots: { default: 'groupName' }, title: '分组', width: 120 },
  { align: 'left', field: 'name', title: '名称', width: 160 },
  { align: 'left', field: 'code', title: '配置编码', width: 280 },
  { align: 'left', field: 'value', title: '当前值', width: 180 },
  { align: 'left', field: 'defaultValue', slots: { default: 'defaultValue' }, title: '默认值', width: 160 },
  { field: 'providerName', slots: { default: 'providerName' }, title: '作用域', width: 110 },
  { field: 'valueType', slots: { default: 'valueType' }, title: '类型', width: 90 },
  { align: 'left', field: 'description', title: '说明' },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 100 },
] satisfies VxeGridProps<ConfigApi.ConfigItem>['columns'];

const items = ref<ConfigApi.ConfigItem[]>([]);
const loading = ref(false);
const pagination = useLocalVxePagination(items, { defaultPageSize: 10 });
const saving = ref(false);
const drawerVisible = ref(false);
const editingItem = ref<ConfigApi.ConfigItem | null>(null);
const formState = reactive({
  numberValue: 0,
  providerName: 'G',
  value: '',
});

const groupFilters = computed(() =>
  [...new Map(items.value.map((item) => [item.groupName, item.groupName])).values()]
    .map((groupName) => ({ label: groupName, value: groupName })),
);

const gridOptions = computed<VxeGridProps<ConfigApi.ConfigItem>>(() => ({
  columns: columns.map((column) =>
    column.field === 'groupName'
      ? {
          ...column,
          filterMethod: ({ option, row }) => row.groupName === option.value,
          filters: groupFilters.value,
        }
      : column,
  ),
  data: pagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: pagination.pagerConfig.value,
  rowConfig: { keyField: 'code' },
}));

const gridEvents: NativeVxeGridEvents<ConfigApi.ConfigItem> = {
  'page-change': pagination.handlePageChange,
};

const [Grid] = useVbenVxeGrid<ConfigApi.ConfigItem>({
  gridEvents,
  gridOptions: {},
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
  formState.providerName = item.providerName || 'G';
  formState.value = item.value;
  formState.numberValue = Number.parseInt(item.value, 10) || 0;
  drawerVisible.value = true;
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
    await updatePlatformConfigApi(editingItem.value.code, {
      providerKey: formState.providerName === 'T' ? editingItem.value.providerKey : null,
      providerName: formState.providerName,
      value: nextValue,
    });
    message.success('配置已保存');
    drawerVisible.value = false;
    await loadConfigs();
  } finally {
    saving.value = false;
  }
}

onMounted(loadConfigs);
</script>

<template>
  <Page description="配置参数用于维护允许在线调整的安全运行参数，基础设施连接与密钥仍应通过配置文件或环境变量管理。" title="配置参数">
    <Card :bordered="false" class="rounded-2xl" title="参数列表">
      <Grid
        class="app-vxe-card-grid"
        :grid-events="gridEvents"
        :grid-options="gridOptions"
      >
        <template #groupName="{ row }">
          <Tag>{{ row.groupName }}</Tag>
        </template>
        <template #providerName="{ row }">
          <Tag :color="row.providerName === 'T' ? 'purple' : 'default'">
            {{ row.providerName === 'T' ? '租户覆盖' : '全局' }}
          </Tag>
        </template>
        <template #defaultValue="{ row }">
          <span>{{ row.defaultValue || '-' }}</span>
        </template>
        <template #valueType="{ row }">
          <Tag color="blue">{{ row.valueType }}</Tag>
        </template>
        <template #actions="{ row }">
          <Space>
            <Button :disabled="row.isReadonly" size="small" type="link" @click="openEdit(row)">
              编辑
            </Button>
          </Space>
        </template>
      </Grid>
    </Card>

    <Drawer
      v-model:open="drawerVisible"
      :title="editingItem ? `编辑配置 · ${editingItem.name}` : '编辑配置'"
      destroy-on-close
      width="560"
    >
      <Form layout="vertical">
        <Form.Item label="配置编码">
          <Input :value="editingItem?.code || ''" disabled />
        </Form.Item>
        <Form.Item label="保存作用域">
          <Radio.Group v-model:value="formState.providerName">
            <Radio.Button value="G">全局</Radio.Button>
            <Radio.Button :disabled="!editingItem?.providerKey" value="T">当前租户</Radio.Button>
          </Radio.Group>
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
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="drawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="handleSubmit">保存</Button>
        </div>
      </template>
    </Drawer>
  </Page>
</template>
