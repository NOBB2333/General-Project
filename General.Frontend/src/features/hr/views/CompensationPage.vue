<script setup lang="ts">
import { ref } from 'vue';
import { Eye, EyeOff, Heart, LockKeyhole, ReceiptText, WalletCards } from 'lucide-vue-next';
import { UiButton, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { benefits, payslips } from '../model/compensation.data';
import type { CompensationMode, CompensationView } from '../model/compensation.types';
import CompensationHero from './compensation/CompensationHero.vue';
import CompensationOverview from './compensation/CompensationOverview.vue';
const hidden = ref(false);
const mode = ref<CompensationMode>('我的薪酬');
const view = ref<CompensationView>('薪酬概览');
const amount = (value: string) => (hidden.value ? '••••••' : value);
const setMode = (value: string) => {
  mode.value = value as CompensationMode;
};
const setView = (value: string) => {
  view.value = value as CompensationView;
};
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="board"
    data-page-route="/hr/compensation"
    data-page-label="薪酬福利"
    data-page-module="人事管理"
    data-compensation-page
    :data-mode="mode"
    :data-view="view"
    :data-hidden="hidden"
  >
    <CompensationHero />
    <div :class="$style.controls">
      <UiButton
        size="sm"
        @click="hidden = !hidden"
        ><template #leading
          ><Eye
            v-if="hidden"
            :size="14"
            :stroke-width="1.8" /><EyeOff
            v-else
            :size="14"
            :stroke-width="1.8" /></template
        >{{ hidden ? '显示金额' : '隐藏金额' }}</UiButton
      ><UiTabs
        :model-value="mode"
        @update:model-value="setMode"
        ><UiTabsList label="薪酬模式"
          ><UiTabsTrigger value="我的薪酬"
            ><WalletCards
              :size="14"
              :stroke-width="1.8"
            />我的薪酬</UiTabsTrigger
          ><UiTabsTrigger value="管理后台"
            ><LockKeyhole
              :size="14"
              :stroke-width="1.8"
            />管理后台</UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
    </div>
    <template v-if="mode === '我的薪酬'">
      <section :class="$style.profile">
        <span>李</span>
        <div>
          <strong>李志强</strong>
          <p>工程部 · 高级工程师 · P6 · 入职 2020-07-01</p>
        </div>
        <dl>
          <div>
            <dt>本月实发</dt>
            <dd>{{ amount('¥19,834.00') }}</dd>
          </div>
          <div>
            <dt>本月税前</dt>
            <dd>{{ amount('¥27,700.00') }}</dd>
          </div>
          <div>
            <dt>年度累计(税前)</dt>
            <dd>{{ amount('¥10.4万') }}</dd>
          </div>
        </dl>
      </section>
      <nav :class="$style.viewTabs">
        <UiTabs
          :model-value="view"
          @update:model-value="setView"
          ><UiTabsList label="薪酬内容"
            ><UiTabsTrigger value="薪酬概览"
              ><WalletCards
                :size="14"
                :stroke-width="1.8"
              />薪酬概览</UiTabsTrigger
            ><UiTabsTrigger value="工资条"
              ><ReceiptText
                :size="14"
                :stroke-width="1.8"
              />工资条</UiTabsTrigger
            ><UiTabsTrigger value="福利明细"
              ><Heart
                :size="14"
                :stroke-width="1.8"
              />福利明细</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </nav>
      <CompensationOverview
        v-if="view === '薪酬概览'"
        :hidden="hidden"
      />
      <section
        v-else-if="view === '工资条'"
        :class="$style.payslips"
      >
        <header>
          <h2>工资条</h2>
          <span>最近 3 个月</span>
        </header>
        <table>
          <thead>
            <tr>
              <th>月份</th>
              <th>税前收入</th>
              <th>扣除合计</th>
              <th>实发金额</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in payslips"
              :key="row.id"
            >
              <td>{{ row.month }}</td>
              <td>{{ amount(row.gross) }}</td>
              <td>{{ amount(row.deductions) }}</td>
              <td>
                <strong>{{ amount(row.net) }}</strong>
              </td>
              <td>
                <span>{{ row.status }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
      <section
        v-else
        :class="$style.benefits"
      >
        <article
          v-for="item in benefits"
          :key="item.id"
        >
          <Heart
            :size="18"
            :stroke-width="1.8"
          /><span>{{ item.label }}</span
          ><strong>{{ amount(item.value) }}</strong
          ><small>{{ item.note }}</small>
        </article>
      </section>
    </template>
    <section
      v-else
      :class="$style.adminPanel"
    >
      <header>
        <div>
          <h2>薪酬管理后台</h2>
          <p>薪资核算、社保公积金与福利计划管理</p>
        </div>
        <UiButton
          variant="primary"
          size="sm"
          >发起本月核算</UiButton
        >
      </header>
      <div>
        <article><span>待核算员工</span><strong>38</strong><small>2026年3月</small></article>
        <article><span>待复核项目</span><strong>4</strong><small>薪资变动</small></article>
        <article><span>社保待申报</span><strong>2</strong><small>本月截止</small></article>
        <article>
          <span>预计发薪总额</span><strong>{{ amount('¥78.3万') }}</strong
          ><small>38 人</small>
        </article>
      </div>
    </section>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1280px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.controls {
  @apply flex h-[66px] items-center justify-end gap-2;
}
.controls [role='tablist'] {
  @apply rounded bg-white p-1 shadow-sm;
}
.controls [role='tab'] {
  @apply min-h-8;
}
.profile {
  @apply flex h-[78px] items-center rounded border bg-white px-5;
  border-color: var(--ui-border);
}
.profile > span {
  @apply flex h-11 w-11 items-center justify-center rounded-full text-[16px] font-semibold text-white;
  background: var(--app-primary);
}
.profile > div {
  @apply ml-4;
}
.profile strong {
  @apply text-[14px];
}
.profile p {
  @apply m-0 mt-1 text-[10px];
  color: var(--ui-text-muted);
}
.profile dl {
  @apply ml-auto flex gap-8;
}
.profile dl div {
  @apply text-right;
}
.profile dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.profile dd {
  @apply m-0 mt-1 text-[15px] font-semibold;
  color: var(--app-primary);
}
.profile dl div:nth-child(2) dd {
  color: var(--ui-success);
}
.profile dl div:nth-child(3) dd {
  color: #6554c0;
}
.viewTabs {
  @apply mt-4 flex h-11 items-end border-b;
  border-color: var(--ui-border);
}
.viewTabs [role='tab'] {
  @apply h-10 rounded-none px-4;
}
.viewTabs [role='tab'][data-state='active'] {
  @apply border-b-2 bg-transparent;
  border-color: var(--app-primary);
}
.payslips,
.adminPanel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.payslips header,
.adminPanel header {
  @apply flex h-14 items-center justify-between border-b px-5;
  border-color: var(--ui-border);
}
.payslips h2,
.adminPanel h2 {
  @apply m-0 text-[14px];
}
.payslips header span,
.adminPanel p {
  @apply m-0 text-[10px];
  color: var(--ui-text-subtle);
}
.payslips table {
  @apply w-full border-collapse text-left text-[11px];
}
.payslips th,
.payslips td {
  @apply h-12 border-b px-5;
  border-color: var(--ui-border-subtle);
}
.payslips th {
  @apply font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
}
.payslips td strong {
  color: var(--app-primary);
}
.payslips td span {
  @apply rounded px-2 py-1 text-[9px] font-semibold;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.benefits,
.adminPanel > div {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.benefits article,
.adminPanel article {
  @apply flex min-h-[124px] flex-col rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.benefits svg {
  color: var(--ui-danger);
}
.benefits span,
.adminPanel article span {
  @apply mt-3 text-[10px];
  color: var(--ui-text-muted);
}
.benefits strong,
.adminPanel article strong {
  @apply mt-2 text-[18px];
  color: var(--app-primary);
}
.benefits small,
.adminPanel article small {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.adminPanel {
  @apply mt-0;
}
.adminPanel > div {
  @apply m-0 p-5;
}
.adminPanel article {
  background: var(--ui-surface-subtle);
}
</style>
