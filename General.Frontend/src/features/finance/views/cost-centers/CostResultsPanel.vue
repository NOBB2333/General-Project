<script setup lang="ts">
import type { CostCenterRow } from '../../model/cost-centers.types';

defineProps<{
  rows: readonly CostCenterRow[];
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}
</script>

<template>
  <section :class="$style.panel">
    <header>
      <h2>归集结果看板</h2>
      <p>按成本类型和归集对象查看当前会话的成本分布</p>
    </header>
    <div :class="$style.resultGrid">
      <article>
        <span>物料成本</span>
        <strong>¥38.6万</strong>
        <small>占已归集成本 52%</small>
      </article>
      <article>
        <span>人工与服务</span>
        <strong>¥21.4万</strong>
        <small>占已归集成本 29%</small>
      </article>
      <article>
        <span>制造及其他</span>
        <strong>¥14.8万</strong>
        <small>占已归集成本 19%</small>
      </article>
    </div>
    <div :class="$style.tableShell">
      <table>
        <thead>
          <tr>
            <th>成本编号</th>
            <th>成本名称</th>
            <th>归集对象</th>
            <th>已归集</th>
            <th>状态</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows.filter((item) => item.collectedAmount > 0)"
            :key="row.id"
          >
            <td>
              <strong>{{ row.code }}</strong>
            </td>
            <td>{{ row.name }}</td>
            <td>{{ row.contract }}</td>
            <td>{{ currency(row.collectedAmount) }}</td>
            <td>{{ row.status }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-4 rounded border bg-white p-5;
  border-color: var(--ui-border);
}

.panel header h2 {
  @apply m-0 text-[15px] font-semibold;
}

.panel header p {
  @apply m-0 mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.resultGrid {
  @apply mt-4 grid grid-cols-3 gap-4;
}

.resultGrid article {
  @apply flex min-h-24 flex-col justify-center rounded border px-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.resultGrid span,
.resultGrid small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.resultGrid strong {
  @apply my-2 text-[22px];
  color: var(--app-primary);
}

.tableShell {
  @apply mt-5 overflow-hidden rounded border;
  border-color: var(--ui-border);
}

.tableShell table {
  @apply w-full border-collapse text-left text-[11px];
}

.tableShell th,
.tableShell td {
  @apply h-10 border-b px-4;
  border-color: var(--ui-border-subtle);
}

.tableShell th {
  @apply font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.tableShell td strong {
  color: var(--app-primary);
}
</style>
