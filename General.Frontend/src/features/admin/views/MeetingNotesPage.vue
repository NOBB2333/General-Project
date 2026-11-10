<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { BookOpen, CalendarDays, CheckCircle2, Hourglass, Plus, Search, UsersRound } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { meetingNotesFixture } from '../model/meeting-notes.data';
import type { MeetingNote, MeetingNoteDraft, MeetingStatus, MeetingType } from '../model/meeting-notes.types';
import MeetingNotesTable from './meeting-notes/MeetingNotesTable.vue';

const notes = ref<MeetingNote[]>(cloneReferenceRows(meetingNotesFixture.rows));
const type = ref<MeetingType | '全部类型'>('全部类型');
const status = ref<MeetingStatus | '全部状态'>('全部状态');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const selected = ref<MeetingNote | null>(null);
const draft = reactive<MeetingNoteDraft>({
  title: '',
  type: '项目会议',
  date: '2026-09-16',
  time: '09:00-10:00',
  host: '张经理',
  attendees: '',
  summary: '',
});
const typeOptions: readonly UiSelectOption[] = [
  '全部类型',
  '项目会议',
  '经营分析',
  '技术评审',
  '部门会议',
  '全体会议',
  '专题讨论',
].map((value) => ({ value, label: value }));
const createTypeOptions: readonly UiSelectOption[] = typeOptions.slice(1);
const statusOptions: readonly UiSelectOption[] = ['全部状态', '草稿', '已发送', '已归档'].map((value) => ({
  value,
  label: value,
}));
const filtered = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return notes.value.filter(
    (item) =>
      (type.value === '全部类型' || item.type === type.value) &&
      (status.value === '全部状态' || item.status === status.value) &&
      (!needle || `${item.code} ${item.title} ${item.host}`.toLocaleLowerCase('zh-CN').includes(needle)),
  );
});
function openCreate() {
  Object.assign(draft, {
    title: '',
    type: '项目会议',
    date: '2026-09-16',
    time: '09:00-10:00',
    host: '张经理',
    attendees: '',
    summary: '',
  });
  formOpen.value = true;
}
function createNote() {
  if (!draft.title.trim() || !draft.date.trim() || !draft.attendees.trim()) {
    toast.warning('请填写会议主题、日期和参会人');
    return;
  }
  const attendees = draft.attendees
    .split(/[、,，\s]+/)
    .map((item) => item.trim())
    .filter(Boolean);
  const next = notes.value.length + 1;
  notes.value.unshift({
    id: `meeting-local-${next}`,
    code: `MTG-LOCAL-${String(next).padStart(3, '0')}`,
    title: draft.title.trim(),
    type: draft.type,
    date: draft.date,
    time: draft.time.trim() || '09:00-10:00',
    host: draft.host.trim() || '张经理',
    attendees,
    attendeeExtra: 0,
    todoCount: 0,
    pendingTodoCount: 0,
    status: '草稿',
    summary: draft.summary.trim() || '待补充会议纪要',
  });
  type.value = '全部类型';
  status.value = '全部状态';
  query.value = '';
  formOpen.value = false;
  toast.success('会议纪要已在本地创建');
}
function openDetail(note: MeetingNote) {
  selected.value = note;
  detailOpen.value = true;
}
</script>
<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/admin/meeting-notes"
    data-page-label="会议纪要"
    data-page-module="行政管理"
    data-meeting-notes-page
    :data-result-count="filtered.length"
    :data-total-count="notes.length"
    :data-type-filter="type"
    :data-status-filter="status"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav aria-label="面包屑"><span>行政管理</span><b>›</b><strong>会议纪要</strong></nav>
        <div :class="$style.identity">
          <span :class="$style.heroIcon"
            ><BookOpen
              :size="24"
              :stroke-width="1.8"
          /></span>
          <div>
            <h1>会议纪要</h1>
            <p>会议记录归档、决议追踪、待办事项指派与执行情况复盘</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <i
          v-for="item in 6"
          :key="item"
        /><b>会议决议</b>
      </div>
    </header>
    <section :class="$style.metrics">
      <article>
        <CalendarDays
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>本月会议</span><strong>6</strong><small>2026年2月</small></div>
      </article>
      <article :class="$style.pending">
        <Hourglass
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>待跟进待办</span><strong>9</strong><small>需要关注</small></div>
      </article>
      <article :class="$style.done">
        <CheckCircle2
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>已完成待办</span><strong>3</strong><small>共17项</small></div>
      </article>
      <article :class="$style.people">
        <UsersRound
          :size="17"
          :stroke-width="1.8"
        />
        <div><span>参会人次</span><strong>32</strong><small>本月累计</small></div>
      </article>
    </section>
    <section :class="$style.workspace">
      <header :class="$style.toolbar">
        <UiSelect
          v-model="type"
          label="会议类型"
          :options="typeOptions"
          :class="$style.filter"
        /><UiSelect
          v-model="status"
          label="会议状态"
          :options="statusOptions"
          :class="$style.filter"
        /><label :class="$style.search"
          ><Search
            :size="15"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索会议"
            placeholder="搜索会议..." /></label
        ><UiButton
          variant="primary"
          size="sm"
          @click="openCreate"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建会议纪要</UiButton
        >
      </header>
      <MeetingNotesTable
        :rows="filtered"
        @view="openDetail"
      />
      <footer>共 {{ filtered.length }} 条记录</footer>
    </section>
  </main>
  <UiDialog
    v-model:open="formOpen"
    title="新建会议纪要"
    description="创建后先保存为本地草稿"
    size="lg"
    ><div :class="$style.form">
      <UiFormField
        label="会议主题"
        required
        :class="$style.formWide"
        ><UiInput
          v-model="draft.title"
          aria-label="会议主题" /></UiFormField
      ><UiFormField label="会议类型"
        ><UiSelect
          v-model="draft.type"
          label="会议类型"
          :options="createTypeOptions" /></UiFormField
      ><UiFormField label="主持人"
        ><UiInput
          v-model="draft.host"
          aria-label="会议主持人" /></UiFormField
      ><UiFormField
        label="日期"
        required
        ><UiInput
          v-model="draft.date"
          aria-label="会议日期" /></UiFormField
      ><UiFormField label="时间"
        ><UiInput
          v-model="draft.time"
          aria-label="会议时间" /></UiFormField
      ><UiFormField
        label="参会人"
        required
        :class="$style.formWide"
        ><UiInput
          v-model="draft.attendees"
          aria-label="会议参会人"
          placeholder="多人请用逗号分隔" /></UiFormField
      ><UiFormField
        label="会议摘要"
        :class="$style.formWide"
        ><UiTextarea
          v-model="draft.summary"
          aria-label="会议摘要"
          :rows="3"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="formOpen = false"
        >取消</UiButton
      ><UiButton
        variant="primary"
        @click="createNote"
        >保存草稿</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="detailOpen"
    :title="selected?.title || '会议纪要详情'"
    :description="selected?.code"
    size="lg"
    ><div
      v-if="selected"
      :class="$style.detail"
    >
      <dl>
        <div>
          <dt>会议类型</dt>
          <dd>{{ selected.type }}</dd>
        </div>
        <div>
          <dt>会议时间</dt>
          <dd>{{ selected.date }} {{ selected.time }}</dd>
        </div>
        <div>
          <dt>主持人</dt>
          <dd>{{ selected.host }}</dd>
        </div>
        <div>
          <dt>参会人</dt>
          <dd>{{ selected.attendees.join('、') }}</dd>
        </div>
        <div>
          <dt>待办</dt>
          <dd>{{ selected.todoCount }} 项，{{ selected.pendingTodoCount }} 项待跟进</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selected.status }}</dd>
        </div>
      </dl>
      <p>{{ selected.summary }}</p>
    </div>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
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
  @apply relative mb-3 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #173477, #0b4a9d 52%, #6951aa);
}
.heroCopy {
  @apply relative z-[2];
}
.hero nav {
  @apply mb-4 flex items-center gap-2 text-[10px];
  color: rgba(255, 255, 255, 0.78);
}
.hero nav strong {
  color: white;
}
.identity {
  @apply flex items-center gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgba(255, 255, 255, 0.25);
  background: rgba(255, 255, 255, 0.12);
}
.identity h1 {
  @apply text-[20px] font-semibold;
}
.identity p {
  @apply mt-1 text-[11px];
  color: rgba(255, 255, 255, 0.78);
}
.heroArt {
  @apply absolute inset-y-0 right-[10%] w-[45%];
}
.heroArt i {
  @apply absolute bottom-5 h-12 w-5 rounded-sm;
  background: #58a5c4;
}
.heroArt i:nth-child(1) {
  left: 4%;
  height: 30px;
}
.heroArt i:nth-child(2) {
  left: 10%;
  height: 48px;
}
.heroArt i:nth-child(3) {
  left: 16%;
  height: 64px;
}
.heroArt i:nth-child(4) {
  left: 22%;
  height: 76px;
}
.heroArt i:nth-child(5) {
  left: 28%;
  height: 56px;
}
.heroArt i:nth-child(6) {
  left: 34%;
  height: 70px;
}
.heroArt b {
  @apply absolute right-[8%] top-6 rounded-lg px-8 py-3 text-[10px];
  color: #594823;
  background: #e9c968;
}
.heroArt b::after {
  @apply absolute right-3 top-full h-0 w-0;
  content: '';
  border-left: 15px solid transparent;
  border-top: 12px solid #e9c968;
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metrics article {
  @apply flex min-h-[94px] items-start gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
  color: #0065ff;
}
.metrics span,
.metrics small {
  @apply block;
  color: var(--ui-text-subtle);
}
.metrics span {
  @apply text-[10px];
}
.metrics strong {
  @apply my-1 block text-[20px] font-semibold;
  color: var(--app-primary);
}
.metrics small {
  @apply text-[9px];
}
.pending {
  color: #ff8b00 !important;
}
.pending strong {
  color: #ff8b00 !important;
}
.done {
  color: #00875a !important;
}
.done strong {
  color: #00875a !important;
}
.people {
  color: #6554c0 !important;
}
.people strong {
  color: #6554c0 !important;
}
.workspace {
  @apply overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-14 items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.filter {
  @apply w-[105px];
}
.search {
  @apply ml-auto flex w-[230px] items-center gap-2 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search input {
  @apply border-0 bg-transparent px-0;
  box-shadow: none;
}
.workspace > footer {
  @apply px-4 py-3 text-[10px];
  color: var(--ui-text-subtle);
}
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formWide {
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
.detail > p {
  @apply mt-4 rounded p-4 text-[11px] leading-5;
  background: var(--ui-surface-subtle);
  color: var(--ui-text-muted);
}
</style>
