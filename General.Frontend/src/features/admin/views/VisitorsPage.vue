<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { CalendarDays, Clock3, Plus, Search, UserRoundCheck, Users } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { visitorsFixture } from '../model/visitors.data';
import type { VisitorDraft, VisitorRecord, VisitorStatus } from '../model/visitors.types';

type VisitorTab = '预约登记' | '今日到访' | '访客台账' | '黑名单' | '统计分析';
const rows = ref<VisitorRecord[]>(cloneReferenceRows(visitorsFixture.rows));
const tab = ref<VisitorTab>('预约登记');
const query = ref('');
const type = ref('全部类型');
const status = ref('全部状态');
const formOpen = ref(false);
const draft = reactive<VisitorDraft>({
  visitor: '',
  unit: '',
  type: '商务洽谈',
  reason: '',
  host: '',
  visitAt: '2026-09-17 09:00 · 2h',
});
const typeOptions: readonly UiSelectOption[] = [
  '全部类型',
  '商务洽谈',
  '技术交流',
  '供应商对接',
  '客户参观',
  '面试招聘',
  '设备维护',
].map((value) => ({ value, label: value }));
const statusOptions: readonly UiSelectOption[] = [
  '全部状态',
  '已通过',
  '已签到',
  '待接待人审批',
  '已离开',
  '进行中',
].map((value) => ({ value, label: value }));
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (item) =>
      (type.value === '全部类型' || item.type === type.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle ||
        `${item.code} ${item.visitor} ${item.unit} ${item.host} ${item.reason}`
          .toLocaleLowerCase('zh-CN')
          .includes(needle)),
  );
});
function openCreate() {
  Object.assign(draft, {
    visitor: '',
    unit: '',
    type: '商务洽谈',
    reason: '',
    host: '',
    visitAt: '2026-09-17 09:00 · 2h',
  });
  formOpen.value = true;
}
function saveVisitor() {
  if (!draft.visitor.trim() || !draft.reason.trim()) return toast.warning('请填写访客和来访事由');
  rows.value.unshift({
    id: `visitor-local-${rows.value.length + 1}`,
    code: `VST-2026-${String(rows.value.length + 520).padStart(4, '0')}`,
    visitor: draft.visitor,
    unit: draft.unit || '—',
    type: draft.type,
    reason: draft.reason,
    host: draft.host || '待安排',
    submitter: '张经理',
    visitAt: draft.visitAt,
    status: '待接待人审批',
  });
  formOpen.value = false;
  toast.success('访客预约已写入本地');
}
function approve(row: VisitorRecord) {
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, status: '已通过' as VisitorStatus } : item));
  toast.success('访客预约已在本地通过');
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/visitors"
    data-page-label="访客管理"
    data-page-module="行政管理"
    data-visitors-page
  >
    <header :class="$style.hero">
      <div :class="$style.heroContent">
        <nav>行政管理 <b>›</b> 访客管理</nav>
        <section>
          <span
            ><UserRoundCheck
              :size="25"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>访客管理</h1>
            <p>访客预约、审批通行、签到签离与到访记录，确保访客可追溯、园区有秩序</p>
          </div>
        </section>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i>访客接待中心</i><b /><em />
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <Clock3
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>待审批</span><strong>2</strong>
          <p>含接待人/行政/安保各级</p>
        </div>
      </article>
      <article>
        <CalendarDays
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>今日预约</span><strong>5</strong>
          <p>2026-05-11</p>
        </div>
      </article>
      <article>
        <Users
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>在访人数</span><strong>2</strong>
          <p>已签到未离开</p>
        </div>
      </article>
      <article>
        <UserRoundCheck
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>本月到访</span><strong>10</strong>
          <p>2026-05（按到访日）</p>
        </div>
      </article>
    </section>
    <section :class="$style.surface">
      <header :class="$style.toolbar">
        <nav>
          <button
            v-for="item in [
              ['预约登记', '13'],
              ['今日到访', '5'],
              ['访客台账', '13'],
              ['黑名单', '3'],
              ['统计分析', ''],
            ]"
            :key="item[0]"
            :data-active="tab === item[0]"
            @click="tab = item[0] as VisitorTab"
          >
            {{ item[0] }} <small v-if="item[1]">({{ item[1] }})</small>
          </button>
        </nav>
        <div :class="$style.filters">
          <UiSelect
            v-model="type"
            label="来访类型"
            :options="typeOptions"
            size="sm"
          /><UiSelect
            v-model="status"
            label="访客状态"
            :options="statusOptions"
            size="sm"
          /><label :class="$style.search"
            ><Search
              :size="14"
              :stroke-width="1.8" /><UiInput
              v-model="query"
              aria-label="搜索访客"
              placeholder="访客 / 单位 / 接待人 / 事由..." /></label
          ><UiButton
            variant="primary"
            size="sm"
            @click="openCreate"
            ><template #leading
              ><Plus
                :size="14"
                :stroke-width="1.8" /></template
            >新建访客预约</UiButton
          >
        </div>
      </header>
      <div
        v-if="tab === '预约登记' || tab === '今日到访' || tab === '访客台账'"
        :class="$style.tableWrap"
      >
        <table>
          <thead>
            <tr>
              <th>预约编号</th>
              <th>访客</th>
              <th>来访单位</th>
              <th>来访类型</th>
              <th>事由</th>
              <th>接待人</th>
              <th>提交人</th>
              <th>到访时间</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in filtered"
              :key="row.id"
            >
              <td>
                <strong :class="$style.code">{{ row.code }}</strong>
              </td>
              <td>
                <span :class="$style.avatar">{{ row.visitor.slice(0, 1) }}</span
                ><b>{{ row.visitor }}</b>
              </td>
              <td>
                <b>{{ row.unit }}</b
                ><small>● CRM: C-{{ row.id.slice(-2) }}</small>
              </td>
              <td>
                <span
                  :class="$style.type"
                  :data-type="row.type"
                  >{{ row.type }}</span
                >
              </td>
              <td>
                <b>{{ row.reason }}</b
                ><small>⌖ 瀚海会议室</small>
              </td>
              <td>
                <span :class="$style.person">{{ row.host.slice(0, 1) }}</span
                ><b>{{ row.host }}</b>
              </td>
              <td>{{ row.submitter }}<small>人事行政部</small></td>
              <td>{{ row.visitAt }}</td>
              <td>
                <span
                  :class="$style.status"
                  :data-status="row.status"
                  >{{ row.status }}</span
                >
              </td>
              <td>
                <UiButton
                  variant="link"
                  size="xs"
                  @click="row.status === '待接待人审批' ? approve(row) : toast.success('已打开访客详情')"
                  >{{ row.status === '待接待人审批' ? '通过' : '查看' }}</UiButton
                >
              </td>
            </tr>
          </tbody>
        </table>
        <p
          v-if="!filtered.length"
          :class="$style.empty"
        >
          当前条件下没有访客预约
        </p>
      </div>
      <div
        v-else-if="tab === '黑名单'"
        :class="$style.emptyPanel"
      >
        <Users :size="28" />
        <h2>黑名单</h2>
        <p>当前暂无需要限制进入园区的访客</p>
      </div>
      <div
        v-else
        :class="$style.emptyPanel"
      >
        <CalendarDays :size="28" />
        <h2>到访统计</h2>
        <p>本月到访 10 人次，今日预约 5 人次</p>
      </div>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新建访客预约"
    description="预约数据只写入本地复刻站"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="访客姓名"
        required
        ><UiInput v-model="draft.visitor" /></UiFormField
      ><UiFormField label="来访单位"><UiInput v-model="draft.unit" /></UiFormField
      ><UiFormField label="来访类型"
        ><UiSelect
          v-model="draft.type"
          label="来访类型"
          :options="typeOptions.slice(1)" /></UiFormField
      ><UiFormField label="接待人"><UiInput v-model="draft.host" /></UiFormField
      ><UiFormField
        label="来访事由"
        required
        ><UiInput v-model="draft.reason" /></UiFormField
      ><UiFormField label="到访时间"><UiInput v-model="draft.visitAt" /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="saveVisitor"
        >保存预约</UiButton
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
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(110deg, #102d52, #164d87 55%, #188d77);
}
.heroContent {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-5 text-[10px] font-semibold;
  color: rgba(255, 255, 255, 0.78);
}
.hero nav b {
  @apply mx-1;
}
.hero section {
  @apply flex items-center gap-3;
}
.hero section > span {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.3);
  background: rgba(255, 255, 255, 0.12);
}
.hero h1 {
  @apply text-[20px] font-semibold;
}
.hero p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.8);
}
.heroArt {
  @apply absolute inset-y-0 right-0 w-[58%];
  background: linear-gradient(165deg, transparent 10%, rgba(20, 123, 130, 0.58));
}
.heroArt i {
  @apply absolute right-[24%] top-2 rounded border px-5 py-2 text-[10px] not-italic;
  border-color: rgba(255, 255, 255, 0.3);
  background: rgba(8, 100, 112, 0.55);
}
.heroArt b {
  @apply absolute bottom-[-20px] left-[38%] h-28 w-1;
  background: rgba(255, 255, 255, 0.35);
}
.heroArt em {
  @apply absolute right-[10%] top-12 h-28 w-36 rounded-full;
  background: rgba(83, 208, 155, 0.35);
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[96px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.metrics article > svg {
  @apply rounded p-2;
  box-sizing: content-box;
  color: #ff8b00;
  background: #fff0b3;
}
.metrics article:nth-child(2) > svg {
  color: #0052cc;
  background: #deebff;
}
.metrics article:nth-child(3) > svg {
  color: #00875a;
  background: #e3fcef;
}
.metrics article:nth-child(4) > svg {
  color: #6554c0;
  background: #eae6ff;
}
.metrics span,
.metrics p {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 block text-[20px];
}
.metrics article:nth-child(1) strong {
  color: #ff8b00;
}
.metrics article:nth-child(2) strong {
  color: #0052cc;
}
.metrics article:nth-child(3) strong {
  color: #00875a;
}
.metrics article:nth-child(4) strong {
  color: #6554c0;
}
.metrics p {
  @apply mt-1;
}
.surface {
  @apply rounded border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[68px] items-center border-b px-3;
  border-color: var(--ui-border);
}
.toolbar nav {
  @apply flex h-full items-center;
}
.toolbar nav button {
  @apply h-full border-b-2 border-transparent px-3 text-[11px] font-medium;
  color: var(--ui-text-muted);
}
.toolbar nav button[data-active='true'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.toolbar nav small {
  @apply text-[9px];
}
.filters {
  @apply ml-auto flex items-center gap-2;
}
.filters > div {
  @apply w-[112px];
}
.search {
  @apply flex h-8 w-[235px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.tableWrap {
  @apply overflow-x-auto;
}
.tableWrap table {
  @apply w-full min-w-[1450px] border-collapse text-left text-[10px];
}
.tableWrap th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}
.tableWrap td {
  @apply h-[71px] border-b px-4 whitespace-nowrap;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableWrap td b {
  @apply font-semibold;
  color: var(--ui-text);
}
.tableWrap td small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.code {
  color: var(--app-primary) !important;
}
.avatar,
.person {
  @apply mr-2 inline-flex h-6 w-6 items-center justify-center rounded-full text-[9px] font-semibold text-white;
  background: #34b27b;
}
.person {
  background: var(--app-primary);
}
.type,
.status {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: #0052cc;
  background: #deebff;
}
.type[data-type='技术交流'] {
  color: #00a3bf;
  background: #e6fcff;
}
.type[data-type='供应商对接'] {
  color: #ff8b00;
  background: #fff0b3;
}
.type[data-type='客户参观'] {
  color: #00875a;
  background: #e3fcef;
}
.type[data-type='面试招聘'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-status='已签到'],
.status[data-status='已离开'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='待接待人审批'] {
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='进行中'] {
  color: #6554c0;
  background: #eae6ff;
}
.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}
.emptyPanel {
  @apply flex min-h-[340px] flex-col items-center justify-center text-center;
  color: var(--app-primary);
}
.emptyPanel h2 {
  @apply mt-3 text-[16px];
  color: var(--ui-text);
}
.emptyPanel p {
  @apply mt-2 text-[11px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
