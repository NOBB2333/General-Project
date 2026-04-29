<script lang="ts" setup>
import type { DictApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Space,
  Switch,
  Table,
  Tag,
  message,
} from 'ant-design-vue';

import {
  createDictDataApi,
  createDictTypeApi,
  deleteDictDataApi,
  deleteDictTypeApi,
  getDictDataApi,
  getDictTypesApi,
  updateDictDataApi,
  updateDictTypeApi,
} from '#/api/core';

defineOptions({ name: 'PlatformDictsPage' });

const typeColumns = [
  { dataIndex: 'name', key: 'name', title: '字典名称' },
  { dataIndex: 'code', key: 'code', title: '编码', width: 180 },
  { dataIndex: 'sort', key: 'sort', title: '排序', width: 80 },
  { key: 'actions', title: '操作', width: 150 },
];

const dataColumns = [
  { dataIndex: 'label', key: 'label', title: '标签', width: 160 },
  { dataIndex: 'value', key: 'value', title: '值', width: 160 },
  { dataIndex: 'tagColor', key: 'tagColor', title: '颜色', width: 100 },
  { dataIndex: 'isEnabled', key: 'isEnabled', title: '状态', width: 90 },
  { dataIndex: 'sort', key: 'sort', title: '排序', width: 80 },
  { dataIndex: 'remark', key: 'remark', title: '备注' },
  { key: 'actions', title: '操作', width: 150 },
];

const loadingTypes = ref(false);
const loadingData = ref(false);
const saving = ref(false);
const types = ref<DictApi.DictTypeItem[]>([]);
const dataItems = ref<DictApi.DictDataItem[]>([]);
const selectedTypeId = ref('');
const typeModalVisible = ref(false);
const dataModalVisible = ref(false);
const editingTypeId = ref<null | string>(null);
const editingDataId = ref<null | string>(null);

const selectedType = computed(() => types.value.find((item) => item.id === selectedTypeId.value) || null);

const typeForm = reactive<DictApi.DictTypeSaveInput>({
  code: '',
  name: '',
  remark: '',
  sort: 10,
});

const dataForm = reactive<DictApi.DictDataSaveInput>({
  isEnabled: true,
  label: '',
  remark: '',
  sort: 10,
  tagColor: '',
  value: '',
});

async function loadTypes() {
  loadingTypes.value = true;
  try {
    types.value = await getDictTypesApi();
    if (!selectedTypeId.value && types.value[0]) {
      selectedTypeId.value = types.value[0].id;
    }
    if (selectedTypeId.value && !types.value.some((item) => item.id === selectedTypeId.value)) {
      selectedTypeId.value = types.value[0]?.id || '';
    }
  } finally {
    loadingTypes.value = false;
  }
}

async function loadData() {
  if (!selectedType.value) {
    dataItems.value = [];
    return;
  }

  loadingData.value = true;
  try {
    dataItems.value = await getDictDataApi(selectedType.value.code);
  } finally {
    loadingData.value = false;
  }
}

async function reloadAll() {
  await loadTypes();
  await loadData();
}

function selectType(typeId: string) {
  selectedTypeId.value = typeId;
  void loadData();
}

function openCreateType() {
  editingTypeId.value = null;
  typeForm.code = '';
  typeForm.name = '';
  typeForm.remark = '';
  typeForm.sort = 10;
  typeModalVisible.value = true;
}

function openEditType(item: DictApi.DictTypeItem) {
  editingTypeId.value = item.id;
  typeForm.code = item.code;
  typeForm.name = item.name;
  typeForm.remark = item.remark || '';
  typeForm.sort = item.sort;
  typeModalVisible.value = true;
}

function openCreateData() {
  if (!selectedType.value) {
    message.warning('请先选择字典类型');
    return;
  }

  editingDataId.value = null;
  dataForm.isEnabled = true;
  dataForm.label = '';
  dataForm.remark = '';
  dataForm.sort = 10;
  dataForm.tagColor = '';
  dataForm.value = '';
  dataModalVisible.value = true;
}

function openEditData(item: DictApi.DictDataItem) {
  editingDataId.value = item.id;
  dataForm.isEnabled = item.isEnabled;
  dataForm.label = item.label;
  dataForm.remark = item.remark || '';
  dataForm.sort = item.sort;
  dataForm.tagColor = item.tagColor || '';
  dataForm.value = item.value;
  dataModalVisible.value = true;
}

async function saveType() {
  if (!typeForm.code.trim() || !typeForm.name.trim()) {
    message.warning('请填写字典编码和名称');
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...typeForm,
      code: typeForm.code.trim(),
      name: typeForm.name.trim(),
      remark: typeForm.remark?.trim() || '',
    };
    if (editingTypeId.value) {
      await updateDictTypeApi(editingTypeId.value, payload);
      message.success('字典类型已保存');
    } else {
      const created = await createDictTypeApi(payload);
      selectedTypeId.value = created.id;
      message.success('字典类型已创建');
    }
    typeModalVisible.value = false;
    await reloadAll();
  } finally {
    saving.value = false;
  }
}

async function saveData() {
  if (!selectedType.value) {
    return;
  }
  if (!dataForm.label.trim() || !dataForm.value.trim()) {
    message.warning('请填写标签和值');
    return;
  }

  saving.value = true;
  try {
    const payload = {
      ...dataForm,
      label: dataForm.label.trim(),
      remark: dataForm.remark?.trim() || '',
      tagColor: dataForm.tagColor?.trim() || '',
      value: dataForm.value.trim(),
    };
    if (editingDataId.value) {
      await updateDictDataApi(editingDataId.value, payload);
      message.success('字典项已保存');
    } else {
      await createDictDataApi(selectedType.value.code, payload);
      message.success('字典项已创建');
    }
    dataModalVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDeleteType(item: DictApi.DictTypeItem) {
  await deleteDictTypeApi(item.id);
  message.success('字典类型已删除');
  if (selectedTypeId.value === item.id) {
    selectedTypeId.value = '';
  }
  await reloadAll();
}

async function handleDeleteData(id: string) {
  await deleteDictDataApi(id);
  message.success('字典项已删除');
  await loadData();
}

onMounted(async () => {
  await reloadAll();
});
</script>

<template>
  <Page description="字典管理用于维护项目状态、优先级、文件分类等通用可配置枚举。" title="字典管理">
    <div class="platform-dict">
      <Card :bordered="false" title="字典类型">
        <template #extra>
          <Button type="primary" @click="openCreateType">新增类型</Button>
        </template>

        <Table
          :columns="typeColumns"
          :data-source="types"
          :loading="loadingTypes"
          :pagination="false"
          row-key="id"
          :row-class-name="(record) => record.id === selectedTypeId ? 'platform-dict__row-active' : ''"
          @row="(record) => ({ onClick: () => selectType(record.id) })"
        >
          <template #emptyText>
            <Empty description="暂无字典类型" />
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'actions'">
              <Space>
                <Button size="small" type="link" @click.stop="openEditType(record as DictApi.DictTypeItem)">
                  编辑
                </Button>
                <Popconfirm title="删除类型会同时删除字典项，确认继续？" @confirm="handleDeleteType(record as DictApi.DictTypeItem)">
                  <Button danger size="small" type="link" @click.stop>删除</Button>
                </Popconfirm>
              </Space>
            </template>
          </template>
        </Table>
      </Card>

      <Card :bordered="false" :title="selectedType ? `${selectedType.name} · 字典项` : '字典项'">
        <template #extra>
          <Button :disabled="!selectedType" type="primary" @click="openCreateData">新增字典项</Button>
        </template>

        <Table
          :columns="dataColumns"
          :data-source="dataItems"
          :loading="loadingData"
          :pagination="{ pageSize: 10 }"
          row-key="id"
        >
          <template #emptyText>
            <Empty :description="selectedType ? '暂无字典项' : '请选择字典类型'" />
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'tagColor'">
              <Tag v-if="record.tagColor" :color="record.tagColor">{{ record.tagColor }}</Tag>
              <span v-else>-</span>
            </template>
            <template v-else-if="column.key === 'isEnabled'">
              <Tag :color="record.isEnabled ? 'green' : 'default'">{{ record.isEnabled ? '启用' : '停用' }}</Tag>
            </template>
            <template v-else-if="column.key === 'actions'">
              <Space>
                <Button size="small" type="link" @click="openEditData(record as DictApi.DictDataItem)">
                  编辑
                </Button>
                <Popconfirm title="确认删除该字典项？" @confirm="handleDeleteData(record.id)">
                  <Button danger size="small" type="link">删除</Button>
                </Popconfirm>
              </Space>
            </template>
          </template>
        </Table>
      </Card>
    </div>

    <Modal
      v-model:open="typeModalVisible"
      :confirm-loading="saving"
      :title="editingTypeId ? '编辑字典类型' : '新增字典类型'"
      @ok="saveType"
    >
      <Form layout="vertical">
        <Form.Item label="字典编码" required>
          <Input v-model:value="typeForm.code" :disabled="!!editingTypeId" :maxlength="64" placeholder="project_status" />
        </Form.Item>
        <Form.Item label="字典名称" required>
          <Input v-model:value="typeForm.name" :maxlength="128" />
        </Form.Item>
        <Form.Item label="排序">
          <InputNumber v-model:value="typeForm.sort" :min="0" style="width: 100%" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="typeForm.remark" :maxlength="256" :rows="3" />
        </Form.Item>
      </Form>
    </Modal>

    <Modal
      v-model:open="dataModalVisible"
      :confirm-loading="saving"
      :title="editingDataId ? '编辑字典项' : '新增字典项'"
      @ok="saveData"
    >
      <Form layout="vertical">
        <Form.Item label="标签" required>
          <Input v-model:value="dataForm.label" :maxlength="128" />
        </Form.Item>
        <Form.Item label="值" required>
          <Input v-model:value="dataForm.value" :maxlength="128" />
        </Form.Item>
        <Form.Item label="标签颜色">
          <Input v-model:value="dataForm.tagColor" :maxlength="64" placeholder="green / blue / orange" />
        </Form.Item>
        <Form.Item label="启用状态">
          <Switch v-model:checked="dataForm.isEnabled" checked-children="启用" un-checked-children="停用" />
        </Form.Item>
        <Form.Item label="排序">
          <InputNumber v-model:value="dataForm.sort" :min="0" style="width: 100%" />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea v-model:value="dataForm.remark" :maxlength="256" :rows="3" />
        </Form.Item>
      </Form>
    </Modal>
  </Page>
</template>

<style scoped>
.platform-dict {
  display: grid;
  gap: 16px;
  grid-template-columns: minmax(420px, 0.42fr) minmax(0, 1fr);
}

:deep(.platform-dict__row-active td) {
  background: #e6f4ff;
}

@media (max-width: 1100px) {
  .platform-dict {
    grid-template-columns: 1fr;
  }
}
</style>
