<script setup lang="ts">
import { BadgeDollarSign, FileCheck2, Landmark } from 'lucide-vue-next';
import { UiTabPanel } from '@/components/ui';
import type {
  MyPerformanceBankEntry,
  MyPerformanceLedgerEntry,
  MyPerformancePlan,
} from '../../model/my-performance.types';

defineProps<{
  bankEntries: readonly MyPerformanceBankEntry[];
  ledgerEntries: readonly MyPerformanceLedgerEntry[];
  plans: readonly MyPerformancePlan[];
}>();
</script>

<template>
  <UiTabPanel value="bank">
    <section
      :class="$style.panel"
      data-performance-bank
    >
      <header :class="$style.panelHeader">
        <Landmark
          :size="18"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <div>
          <h2>我的业绩银行</h2>
          <p>查看已确认并计入绩效的业绩记录</p>
        </div>
      </header>
      <div :class="$style.tableShell">
        <table>
          <thead>
            <tr>
              <th>绩效周期</th>
              <th>确认业绩</th>
              <th>入账业绩</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="entry in bankEntries"
              :key="entry.id"
            >
              <td>{{ entry.period }}</td>
              <td>{{ entry.performance }}</td>
              <td>{{ entry.credited }}</td>
              <td>
                <span :class="$style.successBadge">{{ entry.status }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </UiTabPanel>

  <UiTabPanel value="ledger">
    <section
      :class="$style.panel"
      data-performance-ledger
    >
      <header :class="$style.panelHeader">
        <BadgeDollarSign
          :size="18"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <div>
          <h2>奖金台账</h2>
          <p>查看各绩效周期的奖金核算与发放状态</p>
        </div>
      </header>
      <div :class="$style.tableShell">
        <table>
          <thead>
            <tr>
              <th>绩效周期</th>
              <th>奖金类型</th>
              <th>奖金金额</th>
              <th>发放状态</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="entry in ledgerEntries"
              :key="entry.id"
            >
              <td>{{ entry.period }}</td>
              <td>{{ entry.category }}</td>
              <td>{{ entry.amount }}</td>
              <td>
                <span :class="$style.warningBadge">{{ entry.state }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </UiTabPanel>

  <UiTabPanel value="plans">
    <section
      :class="$style.panel"
      data-performance-plans
    >
      <header :class="$style.panelHeader">
        <FileCheck2
          :size="18"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <div>
          <h2>我适用的方案</h2>
          <p>当前岗位与考核周期适用的绩效方案</p>
        </div>
      </header>
      <div :class="$style.planGrid">
        <article
          v-for="plan in plans"
          :key="plan.id"
          :class="$style.planCard"
        >
          <span :class="$style.successBadge">{{ plan.state }}</span>
          <h3>{{ plan.name }}</h3>
          <p>{{ plan.description }}</p>
          <small>{{ plan.period }}</small>
        </article>
      </div>
    </section>
  </UiTabPanel>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-3.5 rounded border bg-white;
  border-color: var(--ui-border);
}

.panelHeader {
  @apply flex items-start gap-3 border-b px-5 py-4;
  border-color: var(--ui-border);
  color: var(--app-primary);
}

.panelHeader h2 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.panelHeader p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.tableShell {
  @apply overflow-x-auto p-4;
}

.tableShell table {
  @apply w-full min-w-[720px] border-collapse text-left text-[12px];
}

.tableShell th {
  @apply border-b px-3 py-2.5 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.tableShell td {
  @apply border-b px-3 py-3;
  color: var(--ui-text);
  border-color: var(--ui-border-subtle);
}

.successBadge,
.warningBadge {
  @apply inline-flex rounded px-2 py-0.5 text-[10px] font-semibold;
}

.successBadge {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.warningBadge {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.planGrid {
  @apply grid grid-cols-2 gap-3 p-4;
}

.planCard {
  @apply rounded border p-4;
  border-color: var(--ui-border);
}

.planCard h3 {
  @apply mt-3 text-[13px] font-semibold;
  color: var(--ui-text);
}

.planCard p {
  @apply mt-1 text-[11px] leading-5;
  color: var(--ui-text-muted);
}

.planCard small {
  @apply mt-3 block text-[10px];
  color: var(--ui-text-subtle);
}
</style>
