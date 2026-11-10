<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { ArrowRight, ExternalLink, QrCode, ScanLine, Search, Wrench } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { scanRepairReferenceFixture } from '../model/scan-repair.data';
import type { DeviceWarrantyStatus, RepairDevice, RepairDraft } from '../model/scan-repair.types';

const devices = cloneReferenceRows(scanRepairReferenceFixture.rows);
const query = ref('');
const warranty = ref<DeviceWarrantyStatus | '全部'>('全部');
const dialogOpen = ref(false);
const activeDevice = ref<RepairDevice | null>(null);
const createdTicket = ref('');
const draft = reactive<RepairDraft>({ contact: '', phone: '', issue: '', urgency: '普通' });
const warrantyOptions: readonly (DeviceWarrantyStatus | '全部')[] = ['全部', '在保', '即将到期', '已过保', '未开始'];
const urgencyOptions: readonly UiSelectOption[] = [
  { value: '普通', label: '普通' },
  { value: '紧急', label: '紧急' },
];
const repairSteps = [
  '客户扫码 / 模拟扫码',
  '填写报修申请',
  '生成「待受理」工单',
  '客服受理 → 派工 → 处理',
  '客户确认 → 回访关闭',
];
const filteredDevices = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return devices.filter(
    (row) =>
      (!needle ||
        `${row.serial} ${row.name} ${row.model} ${row.customer}`.toLocaleLowerCase('zh-CN').includes(needle)) &&
      (warranty.value === '全部' || row.warrantyStatus === warranty.value),
  );
});
function openRepair(device: RepairDevice) {
  activeDevice.value = device;
  Object.assign(draft, { contact: device.owner || '张经理', phone: '', issue: '', urgency: '普通' });
  dialogOpen.value = true;
}
function submitRepair() {
  if (!activeDevice.value || !draft.contact.trim() || !draft.phone.trim() || !draft.issue.trim()) {
    toast.warning('请完整填写联系人、联系电话和故障描述');
    return;
  }
  createdTicket.value = `SR-LOCAL-${String(Date.now()).slice(-6)}`;
  dialogOpen.value = false;
  toast.success(`${createdTicket.value} 已生成，状态为待受理`);
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="cards"
    data-page-route="/service/scan-repair"
    data-page-label="扫码报修入口"
    data-page-module="服务管理"
    data-scan-repair-page
    :data-result-count="filteredDevices.length"
    :data-created-ticket="createdTicket"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>服务管理</span><b>›</b><strong>扫码报修入口</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><QrCode
              :size="23"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>扫码报修入口（模拟）</h1>
            <p>演示客户扫产品追溯码 → 提交服务申请 → 自动生成「待受理」工单进入服务闭环</p>
          </div>
        </div>
      </div>
      <div :class="$style.qrArt">
        <QrCode
          :size="125"
          :stroke-width="1.4"
        />
      </div>
      <span :class="$style.heroBadge">服务工单</span>
    </header>
    <section
      :class="$style.steps"
      aria-label="扫码报修流程"
    >
      <template
        v-for="(step, index) in repairSteps"
        :key="step"
      >
        <strong v-if="index === 0">{{ step }}</strong>
        <span v-else>{{ step }}</span>
        <ArrowRight
          v-if="index < repairSteps.length - 1"
          :size="15"
        />
      </template>
    </section>
    <section :class="$style.filters">
      <label :class="$style.search"
        ><Search :size="16" /><UiInput
          v-model="query"
          aria-label="搜索设备"
          placeholder="追溯码 / 产品 / 型号 / 客户" /></label
      ><button
        v-for="item in warrantyOptions"
        :key="item"
        type="button"
        :class="[$style.filterButton, warranty === item && $style.filterActive]"
        @click="warranty = item"
      >
        {{ item }}</button
      ><span>共 {{ devices.length }} 台设备</span>
    </section>
    <section :class="$style.grid">
      <article
        v-for="device in filteredDevices"
        :key="device.id"
        :class="$style.card"
      >
        <div :class="$style.qr">
          <i
            v-for="cell in 25"
            :key="cell"
          />
        </div>
        <div :class="$style.cardBody">
          <header>
            <h2>{{ device.name }}</h2>
            <span :class="[$style.status, $style[`status_${device.warrantyStatus}`]]">{{ device.warrantyStatus }}</span>
          </header>
          <p>{{ device.model }} · {{ device.specification }}</p>
          <p>
            {{ device.customer }}<template v-if="device.owner"> · {{ device.owner }}</template>
          </p>
          <p v-if="device.warrantyDate">质保至 {{ device.warrantyDate }}</p>
          <small>{{ device.serial }}</small>
          <div :class="$style.cardActions">
            <UiButton
              variant="primary"
              size="sm"
              @click="openRepair(device)"
              ><template #leading><ScanLine :size="14" /></template>模拟扫码报修</UiButton
            ><UiButton
              variant="secondary"
              size="sm"
              @click="toast.info(`${device.serial} 标签页已在本地打开`)"
              ><template #leading><ExternalLink :size="14" /></template>新标签打开</UiButton
            >
          </div>
        </div>
      </article>
    </section>
  </main>
  <UiDialog
    v-model:open="dialogOpen"
    title="提交报修申请"
    :description="activeDevice ? `${activeDevice.name} · ${activeDevice.serial}` : ''"
    size="md"
    ><div :class="$style.form">
      <UiFormField
        label="联系人"
        required
        ><UiInput
          v-model="draft.contact"
          aria-label="联系人" /></UiFormField
      ><UiFormField
        label="联系电话"
        required
        ><UiInput
          v-model="draft.phone"
          aria-label="联系电话"
          placeholder="请输入手机号" /></UiFormField
      ><UiFormField label="紧急程度"
        ><UiSelect
          v-model="draft.urgency"
          label="紧急程度"
          :options="urgencyOptions" /></UiFormField
      ><UiFormField
        label="故障描述"
        required
        ><UiTextarea
          v-model="draft.issue"
          aria-label="故障描述"
          placeholder="请描述故障现象、发生时间和设备状态"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="dialogOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="submitRepair"
        ><template #leading><Wrench :size="14" /></template>提交并生成工单</UiButton
      ></template
    ></UiDialog
  >
</template>
<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #102a5c 0%, #0747a6 56%, #00b8d9 100%);
}
.hero::before {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: radial-gradient(circle at 20% 8%, rgb(255 255 255 / 13%) 0 2px, transparent 3px);
  background-size: 120px 120px;
}
.heroCopy {
  @apply relative z-[1];
}
.hero nav {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 80%);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 25%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 88%);
}
.qrArt {
  @apply pointer-events-none absolute right-[270px] top-[-8px] opacity-45;
}
.heroBadge {
  @apply absolute right-6 top-[59px] rounded border px-3 py-2 text-[12px];
  border-color: rgb(255 255 255 / 35%);
  background: rgb(255 255 255 / 12%);
}
.steps {
  @apply mb-4 flex min-h-[58px] items-center gap-2 rounded border px-4 text-[12px];
  border-color: #e9e6ff;
  background: #faf9ff;
  color: var(--ui-text-muted);
}
.steps strong,
.steps span {
  @apply rounded border bg-white px-3 py-2;
  border-color: var(--ui-border);
}
.steps strong {
  color: #5243aa;
}
.steps > svg {
  color: #97a0af;
}
.filters {
  @apply mb-3 flex h-10 items-center gap-2;
}
.search {
  @apply flex h-9 w-[315px] items-center gap-2 rounded border bg-white px-3;
  color: #97a0af;
  border-color: var(--ui-border);
}
.search :global(input) {
  @apply min-w-0 flex-1 border-0 p-0 outline-none;
}
.filterButton {
  @apply h-9 rounded-full border bg-white px-3 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.filterActive {
  @apply border-transparent text-white;
  background: var(--app-primary);
}
.filters > span {
  @apply ml-auto text-[11px];
  color: var(--ui-text-subtle);
}
.grid {
  @apply grid grid-cols-3 gap-3;
}
.card {
  @apply flex min-h-[150px] rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.qr {
  @apply grid h-[100px] w-[100px] shrink-0 grid-cols-5 grid-rows-5 gap-[2px] border-4 border-white p-1 shadow;
  background: #f4f5f7;
}
.qr i {
  background: #091e42;
}
.qr i:nth-child(3n),
.qr i:nth-child(7n) {
  background: white;
}
.qr i:nth-child(1),
.qr i:nth-child(5),
.qr i:nth-child(21),
.qr i:nth-child(25) {
  @apply border-4 border-[#091E42] bg-white;
}
.cardBody {
  @apply ml-4 min-w-0 flex-1;
}
.cardBody header {
  @apply flex items-center gap-2;
}
.cardBody h2 {
  @apply truncate text-[13px] font-semibold;
}
.status {
  @apply shrink-0 rounded px-1.5 py-0.5 text-[9px];
}
.status_在保 {
  color: #00875a;
  background: #e3fcef;
}
.status_即将到期 {
  color: #ff8b00;
  background: #fffae6;
}
.status_已过保 {
  color: #de350b;
  background: #ffebe6;
}
.status_未开始 {
  color: #6b778c;
  background: #f4f5f7;
}
.cardBody p {
  @apply mt-1 truncate text-[10px];
  color: var(--ui-text-subtle);
}
.cardBody small {
  @apply mt-2 block text-[9px];
  color: #6b778c;
}
.cardActions {
  @apply mt-2 flex items-center gap-2;
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.form > :last-child {
  @apply col-span-2;
}
</style>
