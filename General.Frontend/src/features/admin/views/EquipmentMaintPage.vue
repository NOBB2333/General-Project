<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import {
  BellRing,
  Box,
  CalendarDays,
  CheckCircle2,
  CircleDollarSign,
  ClipboardList,
  FileBarChart,
  Plus,
  Search,
  Settings2,
  TriangleAlert,
  Wrench,
} from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { initialMaintenanceOrders, maintEquipmentFixture, maintenanceRules } from '../model/equipment-maint.data';
import type {
  EquipmentStatus,
  MaintEquipment,
  MaintEquipmentDraft,
  MaintenanceDraft,
  MaintenanceOrder,
} from '../model/equipment-maint.types';
import EquipmentMaintTable from './equipment-maint/EquipmentMaintTable.vue';

type Section = 'ledger' | 'orders' | 'rules' | 'reminders' | 'costs';
const equipment = ref<MaintEquipment[]>(cloneReferenceRows(maintEquipmentFixture.rows));
const orders = ref<MaintenanceOrder[]>(cloneReferenceRows(initialMaintenanceOrders));
const section = ref<Section>('ledger');
const status = ref<EquipmentStatus | '全部状态'>('全部状态');
const category = ref('全部分类');
const query = ref('');
const equipmentOpen = ref(false);
const maintenanceOpen = ref(false);
const detailOpen = ref(false);
const deleteOpen = ref(false);
const editingId = ref<string | null>(null);
const selected = ref<MaintEquipment | null>(null);
const deleteCandidate = ref<MaintEquipment | null>(null);
const equipmentDraft = reactive<MaintEquipmentDraft>({
  code: '',
  name: '',
  model: '',
  manufacturer: '',
  category: '加工设备',
  location: '',
  owner: '',
  assetValue: '',
});
const maintenanceDraft = reactive<MaintenanceDraft>({ equipment: '', kind: '保养', description: '', owner: '' });
const statusOptions: readonly UiSelectOption[] = ['全部状态', '运行中', '维修中', '闲置', '已报废'].map((value) => ({
  value,
  label: value,
}));
const categoryOptions: readonly UiSelectOption[] = [
  '全部分类',
  '加工设备',
  '焊接设备',
  '动力设备',
  '检测设备',
  '搬运设备',
].map((value) => ({ value, label: value }));
const createCategoryOptions = categoryOptions.slice(1);
const kindOptions: readonly UiSelectOption[] = [
  { value: '维修', label: '维修' },
  { value: '保养', label: '保养' },
];
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return equipment.value.filter(
    (item) =>
      (status.value === '全部状态' || item.status === status.value) &&
      (category.value === '全部分类' || item.category === category.value) &&
      (!needle || `${item.code} ${item.name} ${item.location}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openEquipment(item?: MaintEquipment) {
  editingId.value = item?.id ?? null;
  Object.assign(
    equipmentDraft,
    item
      ? {
          code: item.code,
          name: item.name,
          model: item.model,
          manufacturer: item.manufacturer,
          category: item.category,
          location: item.location,
          owner: item.owner,
          assetValue: String(item.assetValue),
        }
      : {
          code: `EQ-${String(equipment.value.length + 1).padStart(3, '0')}`,
          name: '',
          model: '',
          manufacturer: '',
          category: '加工设备',
          location: '',
          owner: '',
          assetValue: '',
        },
  );
  equipmentOpen.value = true;
}
function saveEquipment() {
  const value = Number(equipmentDraft.assetValue);
  if (!equipmentDraft.name.trim() || !Number.isFinite(value)) return toast.warning('请填写设备名称和有效资产原值');
  if (editingId.value) {
    equipment.value = equipment.value.map((item) =>
      item.id === editingId.value ? { ...item, ...equipmentDraft, assetValue: value } : item,
    );
  } else {
    equipment.value.unshift({
      id: `me-local-${equipment.value.length + 1}`,
      ...equipmentDraft,
      assetValue: value,
      status: '运行中',
      lastMaintenance: '-',
      nextMaintenance: '2026-12-17',
    });
  }
  equipmentOpen.value = false;
  toast.success(editingId.value ? '设备台账已更新' : '设备已添加到本地台账');
}
function openMaintenance(item?: MaintEquipment, kind: '维修' | '保养' = '保养') {
  Object.assign(maintenanceDraft, { equipment: item?.name ?? '', kind, description: '', owner: item?.owner ?? '' });
  maintenanceOpen.value = true;
}
function saveMaintenance() {
  if (!maintenanceDraft.equipment.trim() || !maintenanceDraft.description.trim())
    return toast.warning('请填写设备和工单内容');
  orders.value.unshift({
    id: `wo-local-${orders.value.length + 1}`,
    code: `WO-2026-${String(orders.value.length + 33).padStart(4, '0')}`,
    ...maintenanceDraft,
    status: '待处理',
    cost: 0,
  });
  maintenanceOpen.value = false;
  section.value = 'orders';
  toast.success('维保工单已创建到本地');
}
function openDetail(item: MaintEquipment) {
  selected.value = item;
  detailOpen.value = true;
}
function requestDelete(item: MaintEquipment) {
  deleteCandidate.value = item;
  deleteOpen.value = true;
}
function confirmDelete() {
  if (deleteCandidate.value) equipment.value = equipment.value.filter((item) => item.id !== deleteCandidate.value?.id);
  deleteOpen.value = false;
  toast.success('设备已从本地台账删除');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/equipment-maint"
    data-page-label="设备维护"
    data-page-module="行政管理"
    data-equipment-maint-page
    :data-section="section"
    :data-result-count="filtered.length"
    :data-total-count="equipment.length"
  >
    <header :class="$style.hero">
      <div>
        <nav>行政管理 <b>›</b> 设备维护</nav>
        <section>
          <span
            ><Wrench
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>设备维护保养</h1>
            <p>设备台账管理、维护/维修工单与到期提醒，保障设备稳定运行</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i /><i /><i /><b /><em />
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <span
          ><Box
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>设备总数</p>
          <strong>8</strong><small>在线 5 台</small>
        </div>
      </article>
      <article>
        <span
          ><CheckCircle2
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>运行中</p>
          <strong>5</strong><small>正常运转</small>
        </div>
      </article>
      <article>
        <span
          ><Wrench
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>维修中</p>
          <strong>1</strong><small>待修复</small>
        </div>
      </article>
      <article>
        <span
          ><TriangleAlert
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>逾期保养</p>
          <strong>2</strong><small>需立即处理</small>
        </div>
      </article>
      <article>
        <span
          ><CalendarDays
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>本月工单</p>
          <strong>5</strong><small>2026年3月</small>
        </div>
      </article>
      <article>
        <span
          ><CircleDollarSign
            :size="16"
            :stroke-width="1.8"
        /></span>
        <div>
          <p>年度费用</p>
          <strong>¥4.7万</strong><small>2026年累计</small>
        </div>
      </article>
    </section>
    <header :class="$style.navigation">
      <UiTabs v-model="section"
        ><UiTabsList
          label="设备维护业务区"
          :class="$style.tabs"
          ><UiTabsTrigger
            value="ledger"
            :class="$style.tab"
            ><ClipboardList
              :size="14"
              :stroke-width="1.8"
            />设备台账</UiTabsTrigger
          ><UiTabsTrigger
            value="orders"
            :class="$style.tab"
            >工单管理 <em>{{ orders.length }}</em></UiTabsTrigger
          ><UiTabsTrigger
            value="rules"
            :class="$style.tab"
            ><Settings2
              :size="14"
              :stroke-width="1.8"
            />维保规则</UiTabsTrigger
          ><UiTabsTrigger
            value="reminders"
            :class="$style.tab"
            ><BellRing
              :size="14"
              :stroke-width="1.8"
            />提醒中心 <em>3</em></UiTabsTrigger
          ><UiTabsTrigger
            value="costs"
            :class="$style.tab"
            ><FileBarChart
              :size="14"
              :stroke-width="1.8"
            />费用统计</UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      ><UiButton
        variant="secondary"
        size="sm"
        @click="openMaintenance(undefined, '维修')"
        >新增维修</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="openMaintenance(undefined, '保养')"
        ><template #leading
          ><Plus
            :size="14"
            :stroke-width="1.8" /></template
        >新增维护</UiButton
      >
    </header>
    <template v-if="section === 'ledger'"
      ><section :class="$style.toolbar">
        <label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索维护设备"
            placeholder="搜索设备名称、编号、位置..." /></label
        ><UiSelect
          v-model="status"
          label="设备状态"
          :options="statusOptions"
          :class="$style.filter"
        /><UiSelect
          v-model="category"
          label="设备分类"
          :options="categoryOptions"
          :class="$style.filter"
        /><UiButton
          variant="primary"
          size="sm"
          :class="$style.createEquipment"
          @click="openEquipment()"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新增设备</UiButton
        ><UiButton
          variant="secondary"
          size="sm"
          >导入</UiButton
        ><UiButton
          variant="secondary"
          size="sm"
          >模板</UiButton
        >
      </section>
      <EquipmentMaintTable
        :rows="filtered"
        @view="openDetail"
        @edit="openEquipment"
        @maintain="openMaintenance"
        @remove="requestDelete"
    /></template>
    <section
      v-else-if="section === 'orders'"
      :class="$style.alternate"
    >
      <header>
        <h2>工单管理</h2>
        <p>维修与保养任务执行进度</p>
      </header>
      <article
        v-for="item in orders"
        :key="item.id"
      >
        <Wrench
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.code }} · {{ item.equipment }}</strong>
          <p>{{ item.kind }} · {{ item.description }} · {{ item.owner }}</p>
        </div>
        <span :data-status="item.status">{{ item.status }}</span
        ><b>¥{{ item.cost.toLocaleString('zh-CN') }}</b>
      </article>
    </section>
    <section
      v-else-if="section === 'rules'"
      :class="$style.alternate"
    >
      <header>
        <h2>维保规则</h2>
        <p>自动生成周期性保养任务</p>
      </header>
      <article
        v-for="item in maintenanceRules"
        :key="item.id"
      >
        <Settings2
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.cycle }} · 适用 {{ item.equipmentCount }} 台设备</p>
        </div>
        <span data-status="已完成">启用</span>
      </article>
    </section>
    <section
      v-else-if="section === 'reminders'"
      :class="$style.alternate"
    >
      <header>
        <h2>提醒中心</h2>
        <p>逾期保养和待处理工单</p>
      </header>
      <article
        v-for="item in equipment.filter(
          (device) => device.overdue || device.status === '维修中' || device.status === '闲置',
        )"
        :key="item.id"
      >
        <TriangleAlert
          :size="17"
          :stroke-width="1.8"
        />
        <div>
          <strong>{{ item.name }}</strong>
          <p>{{ item.status }} · 下次保养 {{ item.nextMaintenance }}</p>
        </div>
        <UiButton
          size="sm"
          variant="secondary"
          @click="openMaintenance(item)"
          >创建工单</UiButton
        >
      </article>
    </section>
    <section
      v-else
      :class="$style.costPanel"
    >
      <FileBarChart
        :size="28"
        :stroke-width="1.8"
      />
      <h2>年度维保费用</h2>
      <strong>¥47,000</strong>
      <p>维修 ¥31,800 · 保养 ¥15,200</p>
    </section>
  </main>
  <UiDialog
    v-model:open="equipmentOpen"
    :title="editingId ? '编辑设备' : '新增设备'"
    description="维护设备台账基础信息"
    size="lg"
    ><div :class="$style.form">
      <UiFormField label="设备编号"><UiInput v-model="equipmentDraft.code" /></UiFormField
      ><UiFormField
        label="设备名称"
        required
        ><UiInput v-model="equipmentDraft.name" /></UiFormField
      ><UiFormField label="型号"><UiInput v-model="equipmentDraft.model" /></UiFormField
      ><UiFormField label="制造商"><UiInput v-model="equipmentDraft.manufacturer" /></UiFormField
      ><UiFormField label="分类"
        ><UiSelect
          v-model="equipmentDraft.category"
          label="新增设备分类"
          :options="createCategoryOptions" /></UiFormField
      ><UiFormField label="位置"><UiInput v-model="equipmentDraft.location" /></UiFormField
      ><UiFormField label="负责人"><UiInput v-model="equipmentDraft.owner" /></UiFormField
      ><UiFormField label="资产原值"><UiInput v-model="equipmentDraft.assetValue" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="equipmentOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveEquipment"
        >保存设备</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="maintenanceOpen"
    title="新增维保工单"
    description="创建本地维修或保养任务"
    size="md"
    ><div :class="$style.maintenanceForm">
      <UiFormField
        label="设备"
        required
        ><UiInput v-model="maintenanceDraft.equipment" /></UiFormField
      ><UiFormField label="工单类型"
        ><UiSelect
          v-model="maintenanceDraft.kind"
          label="工单类型"
          :options="kindOptions" /></UiFormField
      ><UiFormField
        label="工单内容"
        required
        ><UiInput v-model="maintenanceDraft.description" /></UiFormField
      ><UiFormField label="负责人"><UiInput v-model="maintenanceDraft.owner" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="maintenanceOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveMaintenance"
        >创建工单</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.name || '设备详情'"
    :description="selected?.code"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>型号 / 制造商</dt>
          <dd>{{ selected.model }} / {{ selected.manufacturer }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
        <div>
          <dt>位置</dt>
          <dd>{{ selected.location }}</dd>
        </div>
        <div>
          <dt>负责人</dt>
          <dd>{{ selected.owner }}</dd>
        </div>
        <div>
          <dt>保养周期</dt>
          <dd>{{ selected.lastMaintenance }} ~ {{ selected.nextMaintenance }}</dd>
        </div>
        <div>
          <dt>资产原值</dt>
          <dd>¥{{ selected.assetValue.toLocaleString('zh-CN') }}</dd>
        </div>
      </dl>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
  <UiAlertDialog
    v-model:open="deleteOpen"
    title="删除设备"
    :description="`确认从本地台账删除“${deleteCandidate?.name || ''}”吗？`"
    confirm-label="删除"
    @confirm="confirmDelete"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mx-6 mb-4 mt-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #172b5b, #0754a6 55%, #5243aa);
}
.hero > div:first-child {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.26);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[70%];
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.12) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.12) 1px, transparent 1px);
  background-size: 44px 44px;
}
.heroArt i {
  @apply absolute rounded-full;
  background: #7f8da9;
  border: 8px solid #233e77;
}
.heroArt i:first-child {
  right: 14%;
  top: -18px;
  height: 120px;
  width: 120px;
}
.heroArt i:nth-child(2) {
  right: 30%;
  top: -26px;
  height: 74px;
  width: 74px;
  background: #a46531;
}
.heroArt i:nth-child(3) {
  right: 25%;
  bottom: -24px;
  height: 54px;
  width: 54px;
}
.heroArt b,
.heroArt em {
  @apply absolute h-3 w-3 rounded-full;
  background: #ffe380;
  box-shadow: 0 0 12px #ffe380;
}
.heroArt b {
  right: 25%;
  top: 32px;
}
.heroArt em {
  right: 8%;
  bottom: 6px;
}
.metrics {
  @apply mx-6 mb-4 grid grid-cols-6 gap-3;
}
.metrics article {
  @apply flex min-h-[112px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > span {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(2) > span {
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(3) > span {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(4) > span {
  color: #de350b;
  background: #ffebe6;
}
.metrics article:nth-child(5) > span {
  color: #6554c0;
  background: #eae6ff;
}
.metrics article:nth-child(6) > span {
  color: #ff8b00;
  background: #fff0b3;
}
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply my-2 block text-[20px];
}
.metrics small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.navigation {
  @apply flex h-14 items-center border-b px-8;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-2;
}
.tab {
  @apply rounded-none border-b-2 border-transparent px-2;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: transparent;
}
.tab em {
  @apply rounded-full px-2 py-0.5 text-[8px] not-italic text-white;
  background: #de350b;
}
.navigation > button:first-of-type {
  @apply ml-auto;
}
.navigation > button + button {
  @apply ml-2;
}
.toolbar {
  @apply mx-6 flex h-16 items-center gap-2;
}
.search {
  @apply flex w-[345px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: white;
}
.search input {
  @apply border-0 px-0;
  box-shadow: none;
}
.filter {
  @apply w-[105px];
}
.createEquipment {
  @apply ml-0;
  background: #00875a !important;
}
.alternate {
  @apply mx-6 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.alternate > header {
  @apply border-b p-5;
  border-color: var(--ui-border);
}
.alternate h2 {
  @apply text-[15px] font-semibold;
}
.alternate header p {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.alternate article {
  @apply flex min-h-[68px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border-subtle);
}
.alternate article > svg {
  color: var(--app-primary);
}
.alternate article > div {
  @apply min-w-0 flex-1;
}
.alternate article strong {
  @apply text-[11px];
}
.alternate article p {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.alternate article > span {
  @apply rounded px-2 py-1 text-[8px];
  color: #ff8b00;
  background: #fff0b3;
}
.alternate article > span[data-status='已完成'] {
  color: #00875a;
  background: #e3fcef;
}
.alternate article > b {
  @apply w-20 text-right text-[10px];
}
.costPanel {
  @apply mx-6 rounded border bg-white py-14 text-center;
  color: #6554c0;
  border-color: var(--ui-border);
}
.costPanel h2 {
  @apply mt-3 text-[14px];
  color: var(--ui-text);
}
.costPanel strong {
  @apply mt-3 block text-[28px];
}
.costPanel p {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.maintenanceForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.maintenanceForm > label:nth-child(3) {
  @apply col-span-2;
}
.detail {
  @apply p-5;
}
.detail dl {
  @apply grid grid-cols-2 gap-3;
}
.detail dl div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
}
.detail dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.detail dd {
  @apply mt-1 text-[11px] font-medium;
}
</style>
