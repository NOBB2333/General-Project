<script setup lang="ts">
import { FilePlus2, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiButton, UiInput } from '@/components/ui';
import type { PricingBusinessRecord, PricingTabId } from '../../model/pricing-center.types';

const props = defineProps<{
  tab: Exclude<PricingTabId, 'pricing'>;
  rows: readonly PricingBusinessRecord[];
}>();

const emit = defineEmits<{ create: [tab: Exclude<PricingTabId, 'pricing'>] }>();
const query = ref('');

const panelCopy: Record<Exclude<PricingTabId, 'pricing'>, { title: string; description: string; action: string }> = {
  pricegroup: { title: '价格组管理', description: '按产品系列维护统一的价格规则和商品归属', action: '新建价格组' },
  agreement: { title: '框架协议价', description: '管理客户框架协议中的专属商品价格和有效期', action: '新建协议价' },
  special: { title: '特价审批', description: '审核项目报价中低于标准价格的特价申请', action: '新建特价申请' },
  alert: { title: '价格预警', description: '识别毛利率、成本偏离及长期未调价风险', action: '配置预警规则' },
  adjustment: { title: '价格调整记录', description: '追踪批量调价、审批和生效记录', action: '新建调价单' },
  history: { title: '价格历史', description: '查看商品历史报价、特价及成交价格', action: '导出价格历史' },
};

const copy = computed(() => panelCopy[props.tab]);
const filteredRows = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase();
  if (!needle) return props.rows;
  return props.rows.filter((row) =>
    [row.code, row.title, row.subtitle, row.amount, row.status, row.updatedAt].some((value) =>
      value.toLocaleLowerCase().includes(needle),
    ),
  );
});

function statusTone(status: string) {
  if (['生效中', '已生效', '已通过', '成交', '已分配'].includes(status)) return 'success';
  if (['审批中', '待审批', '报价', '特价', '中风险'].includes(status)) return 'warning';
  if (['已驳回', '已过期', '高风险'].includes(status)) return 'danger';
  return 'neutral';
}
</script>

<template>
  <section :class="$style.panel">
    <header :class="$style.header">
      <div>
        <h2>{{ copy.title }}</h2>
        <p>{{ copy.description }}</p>
      </div>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create', tab)"
      >
        <template #leading
          ><FilePlus2
            :size="14"
            :stroke-width="1.8"
        /></template>
        {{ copy.action }}
      </UiButton>
    </header>

    <div :class="$style.toolbar">
      <label :class="$style.search">
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <UiInput
          v-model="query"
          :placeholder="`搜索${copy.title}…`"
        />
      </label>
      <span>共 {{ filteredRows.length }} 条记录</span>
    </div>

    <div :class="$style.tableWrap">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>编号 / 类型</th>
            <th>名称</th>
            <th>业务说明</th>
            <th>价格 / 指标</th>
            <th>状态</th>
            <th>日期</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in filteredRows"
            :key="row.id"
          >
            <td :class="$style.code">{{ row.code }}</td>
            <td :class="$style.title">{{ row.title }}</td>
            <td :class="$style.subtitle">{{ row.subtitle }}</td>
            <td :class="$style.amount">{{ row.amount }}</td>
            <td>
              <span :class="[$style.status, $style[statusTone(row.status)]]">{{ row.status }}</span>
            </td>
            <td :class="$style.date">{{ row.updatedAt }}</td>
            <td>
              <button
                type="button"
                :class="$style.detail"
              >
                查看
              </button>
            </td>
          </tr>
          <tr v-if="!filteredRows.length">
            <td
              colspan="7"
              :class="$style.empty"
            >
              暂无匹配记录
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.header {
  @apply flex items-start justify-between gap-4 border-b px-5 py-4;
  border-color: var(--ui-border-subtle);
}
.header h2 {
  @apply text-[16px] font-semibold;
  color: var(--ui-text);
}
.header p {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.toolbar {
  @apply flex min-h-[58px] items-center justify-between gap-4 border-b px-4;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.search {
  @apply relative block w-[320px];
}
.search > svg {
  @apply pointer-events-none absolute left-2.5 top-1/2 z-[1] -translate-y-1/2;
  color: #97a0af;
}
.search input {
  @apply h-8 pl-8;
}
.toolbar > span {
  @apply text-[11px];
}
.tableWrap {
  @apply overflow-auto;
}
.table {
  @apply w-full min-w-[1040px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply whitespace-nowrap px-4 py-3 text-[11px] font-semibold;
  color: #5e6c84;
  background: #f7f8f9;
}
.table td {
  @apply border-t px-4 py-3.5 align-middle;
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-subtle);
}
.code {
  @apply whitespace-nowrap font-mono text-[11px];
}
.title {
  @apply min-w-[170px] font-semibold;
  color: var(--ui-text);
}
.subtitle {
  @apply min-w-[290px];
  color: var(--ui-text-subtle);
}
.amount {
  @apply whitespace-nowrap font-semibold tabular-nums;
  color: var(--ui-text);
}
.date {
  @apply whitespace-nowrap tabular-nums;
}
.status {
  @apply inline-flex whitespace-nowrap rounded-full px-2 py-0.5 text-[10px] font-semibold;
}
.success {
  color: var(--ui-success);
  background: #e3fcef;
}
.warning {
  color: #974f0c;
  background: #fffae6;
}
.danger {
  color: var(--ui-danger);
  background: #ffebe6;
}
.neutral {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.detail {
  @apply font-medium hover:underline;
  color: var(--app-primary);
}
.empty {
  @apply py-20 text-center;
  color: #97a0af;
}
</style>
