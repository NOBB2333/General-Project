<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';
import type { DictApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';

import { Page } from '@vben/common-ui';

import {
  Button,
  Drawer,
  Empty,
  Form,
  Input,
  InputNumber,
  Popconfirm,
  Space,
  Switch,
  Tag,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
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
import { useActionLoading } from '#/composables/platform/use-action-loading';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'PlatformDictsPage' });

const DEFAULT_SORT = 1;

const dataColumns = [
  { field: 'order', slots: { default: 'order' }, title: '序号', width: 72 },
  { align: 'left', field: 'label', title: '标签', width: 160 },
  { align: 'left', field: 'value', title: '值', width: 160 },
  { field: 'tagColor', slots: { default: 'tagColor' }, title: '颜色', width: 100 },
  { field: 'isEnabled', slots: { default: 'isEnabled' }, title: '状态', width: 90 },
  { align: 'left', field: 'remark', title: '备注', minWidth: 180 },
  { field: 'actions', fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 210 },
] satisfies VxeGridProps<DictApi.DictDataItem>['columns'];

const loadingTypes = ref(false);
const loadingData = ref(false);
const saving = ref(false);
const types = ref<DictApi.DictTypeItem[]>([]);
const dataItems = ref<DictApi.DictDataItem[]>([]);
const dataPagination = useLocalVxePagination(dataItems, { defaultPageSize: 10 });
const selectedTypeId = ref('');
const typeDrawerVisible = ref(false);
const dataDrawerVisible = ref(false);
const editingTypeId = ref<null | string>(null);
const editingDataId = ref<null | string>(null);
const { actionLoadingKey, runAction } = useActionLoading();

const selectedType = computed(
  () => types.value.find((item) => item.id === selectedTypeId.value) || null,
);
const enabledDataCount = computed(
  () => dataItems.value.filter((item) => item.isEnabled).length,
);
const disabledDataCount = computed(
  () => dataItems.value.length - enabledDataCount.value,
);
const selectedTypeIndex = computed(() =>
  selectedType.value
    ? types.value.findIndex((item) => item.id === selectedType.value?.id) + 1
    : 0,
);
const dataGridOptions = computed<VxeGridProps<DictApi.DictDataItem>>(() => ({
  columns: dataColumns,
  data: dataPagination.pagedRows.value,
  emptyText: selectedType.value ? '暂无字典项' : '请选择字典类型',
  loading: loadingData.value,
  pagerConfig: dataPagination.pagerConfig.value,
  rowClassName: ({ row }) =>
    row.isEnabled ? '' : 'platform-dict__data-row-disabled',
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const dataGridEvents: NativeVxeGridEvents<DictApi.DictDataItem> = {
  'page-change': dataPagination.handlePageChange,
};

const [DataGrid] = useVbenVxeGrid<DictApi.DictDataItem>({
  gridEvents: dataGridEvents,
  gridOptions: {},
});

const typeForm = reactive<DictApi.DictTypeSaveInput>({
  code: '',
  name: '',
  remark: '',
  sort: DEFAULT_SORT,
});

const dataForm = reactive<DictApi.DictDataSaveInput>({
  isEnabled: true,
  label: '',
  remark: '',
  sort: DEFAULT_SORT,
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
    if (
      selectedTypeId.value &&
      !types.value.some((item) => item.id === selectedTypeId.value)
    ) {
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
  dataPagination.resetPage();
  void loadData();
}

function openCreateType() {
  editingTypeId.value = null;
  typeForm.code = '';
  typeForm.name = '';
  typeForm.remark = '';
  typeForm.sort = types.value.length + 1;
  typeDrawerVisible.value = true;
}

function openEditType(item: DictApi.DictTypeItem) {
  editingTypeId.value = item.id;
  typeForm.code = item.code;
  typeForm.name = item.name;
  typeForm.remark = item.remark || '';
  typeForm.sort = item.sort;
  typeDrawerVisible.value = true;
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
  dataForm.sort = dataItems.value.length + 1;
  dataForm.tagColor = '';
  dataForm.value = '';
  dataDrawerVisible.value = true;
}

function openEditData(item: DictApi.DictDataItem) {
  editingDataId.value = item.id;
  dataForm.isEnabled = item.isEnabled;
  dataForm.label = item.label;
  dataForm.remark = item.remark || '';
  dataForm.sort = item.sort;
  dataForm.tagColor = item.tagColor || '';
  dataForm.value = item.value;
  dataDrawerVisible.value = true;
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
    typeDrawerVisible.value = false;
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
    dataDrawerVisible.value = false;
    await loadData();
  } finally {
    saving.value = false;
  }
}

async function handleDeleteType(item: DictApi.DictTypeItem) {
  await runAction(`delete-type:${item.id}`, async () => {
    await deleteDictTypeApi(item.id);
    message.success('字典类型已删除');
    if (selectedTypeId.value === item.id) {
      selectedTypeId.value = '';
    }
    await reloadAll();
  });
}

async function handleDeleteData(id: string) {
  await runAction(`delete-data:${id}`, async () => {
    await deleteDictDataApi(id);
    message.success('字典项已删除');
    await loadData();
  });
}

async function handleToggleDataEnabled(item: DictApi.DictDataItem) {
  await runAction(`toggle-data:${item.id}`, async () => {
    await updateDictDataApi(item.id, {
      isEnabled: !item.isEnabled,
      label: item.label,
      remark: item.remark || '',
      sort: item.sort,
      tagColor: item.tagColor || '',
      value: item.value,
    });
    message.success(item.isEnabled ? '字典项已停用' : '字典项已启用');
    await loadData();
  });
}

onMounted(async () => {
  await reloadAll();
});
</script>

<template>
  <Page
    description="先在左侧选择字典类型，再在右侧维护该类型下的业务选项和值。"
    title="字典管理"
  >
    <div class="platform-dict">
      <section class="platform-dict__type-panel">
        <div class="platform-dict__panel-head">
          <div>
            <span class="platform-dict__eyebrow">类型目录</span>
            <h3>字典类型</h3>
            <p>点击左侧类型，右侧显示该类型下的枚举值。</p>
          </div>
          <Button type="primary" @click="openCreateType">新增类型</Button>
        </div>

        <div v-if="loadingTypes" class="platform-dict__loading">
          正在加载字典类型...
        </div>
        <div v-else-if="types.length > 0" class="platform-dict__type-list">
          <button
            v-for="(item, index) in types"
            :key="item.id"
            :class="[
              'platform-dict__type-item',
              item.id === selectedTypeId ? 'is-active' : '',
            ]"
            type="button"
            @click="selectType(item.id)"
          >
            <span class="platform-dict__type-index">{{ index + 1 }}</span>
            <span class="platform-dict__type-main">
              <strong>{{ item.name }}</strong>
              <span>{{ item.code }}</span>
            </span>
            <span class="platform-dict__type-cue">查看</span>
          </button>
        </div>
        <Empty v-else description="暂无字典类型" />
      </section>

      <section class="platform-dict__data-panel">
        <div
          class="platform-dict__panel-head platform-dict__panel-head--detail"
        >
          <div>
            <span class="platform-dict__eyebrow">当前维护</span>
            <h3>{{ selectedType ? selectedType.name : '请选择字典类型' }}</h3>
            <p v-if="selectedType">
              第 {{ selectedTypeIndex }} 类 / 编码 {{ selectedType.code }}
              <span v-if="selectedType.remark">
                / {{ selectedType.remark }}</span
              >
            </p>
            <p v-else>
              右侧会显示左侧选中类型的字典项，用于下拉框、状态标签和筛选条件。
            </p>
          </div>
          <Space v-if="selectedType" wrap>
            <Button @click="openEditType(selectedType)">编辑类型</Button>
            <Popconfirm
              title="删除类型会同时删除字典项，确认继续？"
              @confirm="handleDeleteType(selectedType)"
            >
              <Button
                danger
                :loading="actionLoadingKey === `delete-type:${selectedType.id}`"
              >
                删除类型
              </Button>
            </Popconfirm>
            <Button type="primary" @click="openCreateData">新增字典项</Button>
          </Space>
          <Button v-else disabled type="primary">新增字典项</Button>
        </div>

        <div class="platform-dict__summary">
          <article>
            <span>全部字典项</span>
            <strong>{{ dataItems.length }}</strong>
          </article>
          <article>
            <span>启用</span>
            <strong>{{ enabledDataCount }}</strong>
          </article>
          <article>
            <span>停用</span>
            <strong>{{ disabledDataCount }}</strong>
          </article>
        </div>

        <DataGrid class="app-vxe-card-grid" :grid-events="dataGridEvents" :grid-options="dataGridOptions">
          <template #order="{ rowIndex }">
            {{ rowIndex + 1 }}
          </template>
          <template #tagColor="{ row }">
            <Tag v-if="row.tagColor" :color="row.tagColor">
              {{ row.tagColor }}
            </Tag>
            <span v-else>-</span>
          </template>
          <template #isEnabled="{ row }">
            <Tag :color="row.isEnabled ? 'green' : 'default'">
              {{ row.isEnabled ? '启用' : '停用' }}
            </Tag>
          </template>
          <template #actions="{ row }">
            <Space>
              <Popconfirm
                :title="
                  row.isEnabled
                    ? '停用后业务下拉不再展示该字典项，确认停用？'
                    : '确认重新启用该字典项？'
                "
                @confirm="handleToggleDataEnabled(row)"
              >
                <Button
                  size="small"
                  type="link"
                  :loading="actionLoadingKey === `toggle-data:${row.id}`"
                >
                  {{ row.isEnabled ? '停用' : '启用' }}
                </Button>
              </Popconfirm>
              <Button
                size="small"
                type="link"
                @click="openEditData(row)"
              >
                编辑
              </Button>
              <Popconfirm
                title="确认删除该字典项？"
                @confirm="handleDeleteData(row.id)"
              >
                <Button
                  danger
                  :loading="actionLoadingKey === `delete-data:${row.id}`"
                  size="small"
                  type="link"
                >
                  删除
                </Button>
              </Popconfirm>
            </Space>
          </template>
        </DataGrid>
      </section>
    </div>

    <Drawer
      v-model:open="typeDrawerVisible"
      :title="editingTypeId ? '编辑字典类型' : '新增字典类型'"
      destroy-on-close
      width="560"
    >
      <Form layout="vertical">
        <Form.Item label="字典编码" required>
          <Input
            v-model:value="typeForm.code"
            :disabled="!!editingTypeId"
            :maxlength="64"
            placeholder="project_status"
          />
        </Form.Item>
        <Form.Item label="字典名称" required>
          <Input v-model:value="typeForm.name" :maxlength="128" />
        </Form.Item>
        <Form.Item label="显示顺序">
          <InputNumber
            v-model:value="typeForm.sort"
            :min="1"
            style="width: 100%"
          />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea
            v-model:value="typeForm.remark"
            :maxlength="256"
            :rows="3"
          />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="typeDrawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="saveType">保存</Button>
        </div>
      </template>
    </Drawer>

    <Drawer
      v-model:open="dataDrawerVisible"
      :title="editingDataId ? '编辑字典项' : '新增字典项'"
      destroy-on-close
      width="560"
    >
      <Form layout="vertical">
        <Form.Item label="标签" required>
          <Input v-model:value="dataForm.label" :maxlength="128" />
        </Form.Item>
        <Form.Item label="值" required>
          <Input v-model:value="dataForm.value" :maxlength="128" />
        </Form.Item>
        <Form.Item label="标签颜色">
          <Input
            v-model:value="dataForm.tagColor"
            :maxlength="64"
            placeholder="green / blue / orange"
          />
        </Form.Item>
        <Form.Item label="启用状态">
          <Switch
            v-model:checked="dataForm.isEnabled"
            checked-children="启用"
            un-checked-children="停用"
          />
        </Form.Item>
        <Form.Item label="显示顺序">
          <InputNumber
            v-model:value="dataForm.sort"
            :min="1"
            style="width: 100%"
          />
        </Form.Item>
        <Form.Item label="备注">
          <Input.TextArea
            v-model:value="dataForm.remark"
            :maxlength="256"
            :rows="3"
          />
        </Form.Item>
      </Form>
      <template #footer>
        <div class="flex justify-end gap-2">
          <Button @click="dataDrawerVisible = false">取消</Button>
          <Button :loading="saving" type="primary" @click="saveData">保存</Button>
        </div>
      </template>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.platform-dict {
  align-items: start;
  display: grid;
  @apply gap-4;
  grid-template-columns: minmax(360px, 0.36fr) minmax(0, 1fr);
}

.platform-dict__type-panel,
.platform-dict__data-panel {
  min-width: 0;
  @apply p-4;
  background: var(--ant-color-bg-container, #fff);
  border: 1px solid var(--ant-color-border-secondary, #e5e7eb);
  @apply rounded-lg;
  box-shadow: 0 1px 2px rgb(15 23 42 / 4%);
}

.platform-dict__panel-head {
  display: flex;
  @apply gap-4;
  align-items: flex-start;
  justify-content: space-between;
  @apply mb-4;
}

.platform-dict__panel-head h3 {
  @apply my-1;
  color: var(--ant-color-text, #1f2937);
  font-size: 16px;
  line-height: 1.3;
}

.platform-dict__panel-head p {
  @apply m-0;
  color: var(--ant-color-text-secondary, #6b7280);
  font-size: 12px;
  line-height: 1.6;
}

.platform-dict__panel-head--detail {
  @apply pb-3.5;
  border-bottom: 1px solid var(--ant-color-border-secondary, #e5e7eb);
}

.platform-dict__eyebrow {
  color: var(--ant-color-text-tertiary, #8a8f98);
  font-size: 12px;
}

.platform-dict__loading {
  @apply py-6;
  color: var(--ant-color-text-secondary, #6b7280);
  text-align: center;
}

.platform-dict__type-list {
  display: grid;
  @apply gap-2;
}

.platform-dict__type-item {
  display: grid;
  width: 100%;
  min-height: 64px;
  @apply px-3 py-2.5;
  color: var(--ant-color-text, #1f2937);
  text-align: left;
  cursor: pointer;
  background: var(--ant-color-bg-container, #fff);
  border: 1px solid var(--ant-color-border-secondary, #e5e7eb);
  @apply rounded-lg;
  grid-template-columns: 28px minmax(0, 1fr) auto;
  @apply gap-2.5;
  align-items: center;
  transition:
    background-color 0.2s ease,
    border-color 0.2s ease,
    box-shadow 0.2s ease;
}

.platform-dict__type-item:hover {
  background: var(--ant-color-bg-container, #fff);
  border-color: var(--ant-color-primary-border, #91caff);
  box-shadow: 0 2px 8px rgb(15 23 42 / 6%);
}

.platform-dict__type-item.is-active {
  background: var(--ant-color-bg-container, #fff);
  border-color: var(--ant-color-primary-border, #91caff);
  box-shadow:
    inset 3px 0 0 var(--ant-color-primary, #1677ff),
    0 2px 10px rgb(22 119 255 / 10%);
}

.platform-dict__type-index {
  display: inline-flex;
  width: 26px;
  height: 26px;
  color: var(--ant-color-text-secondary, #6b7280);
  background: var(--ant-color-fill-quaternary, #f5f7fa);
  @apply rounded-full;
  align-items: center;
  justify-content: center;
  font-size: 12px;
}

.platform-dict__type-main {
  display: grid;
  min-width: 0;
  @apply gap-1;
}

.platform-dict__type-main strong {
  overflow: hidden;
  color: var(--ant-color-text, #1f2937);
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-dict__type-main span {
  overflow: hidden;
  color: var(--ant-color-text-tertiary, #8a8f98);
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.platform-dict__type-cue {
  color: var(--ant-color-text-tertiary, #8a8f98);
  font-size: 12px;
}

.platform-dict__type-item.is-active .platform-dict__type-cue {
  color: var(--ant-color-primary, #1677ff);
}

:deep(.platform-dict__data-row-disabled td) {
  color: var(--ant-color-text-tertiary, #8a8f98);
  background: var(--ant-color-fill-quaternary, #f5f7fa);
}

:deep(.platform-dict__data-row-disabled td:first-child) {
  box-shadow: inset 3px 0 0 var(--ant-color-border, #d9d9d9);
}

.platform-dict__summary {
  display: grid;
  @apply gap-2.5;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  @apply mb-4;
}

.platform-dict__summary article {
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-quaternary, #f5f7fa);
  @apply rounded-lg;
}

.platform-dict__summary span {
  display: block;
  color: var(--ant-color-text-secondary, #6b7280);
  font-size: 12px;
}

.platform-dict__summary strong {
  display: block;
  @apply mt-1;
  color: var(--ant-color-text, #1f2937);
  font-size: 18px;
}

:deep(.platform-dict__data-panel .ant-table) {
  min-width: 0;
}

@media (max-width: 1100px) {
  .platform-dict {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .platform-dict__panel-head {
    display: grid;
  }

  .platform-dict__summary {
    grid-template-columns: 1fr;
  }
}
</style>
